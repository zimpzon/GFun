using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using PlayFab;
using PlayFab.ClientModels;

/// <summary>
/// PlayerData: key/value where value is a string. Could be JSON.
/// PlayerStatistics: key/value where value is an int. PlayFab always creates a leaderboard for these. Min, Max, Sum can be set in PlayFab.com.
/// </summary>
public class PlayFabFacade : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern string GetURLFromPage();

    public static PlayFabFacade Instance;

    public bool LoginWhenInEditor = false;

    [NonSerialized] public object LastResult;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Application.isEditor && !LoginWhenInEditor)
            return;

        StartCoroutine(InitializePlayFab());
    }

    public IEnumerator InitializePlayFab()
    {
        Debug.Log("PlayFab: Login...");
        yield return DoLoginCo();
        if (!PlayFabClientAPI.IsClientLoggedIn())
            yield break;

        Debug.Log("PlayFab: Get all stats...");
        yield return GetAllStats();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("PlayFab: Sending WebGL url info...");
            yield return LogPlayerData(
                new Dictionary<string, string> {
            { "hosting_url", Application.absoluteURL },
            { "page_top_url", GetURLFromPage() }
            });
        }
    }

    string GetUserId()
    {
        var userId = PlayerPrefs.GetString("UserId", string.Empty);
        if (userId == string.Empty)
        {
            userId = Guid.NewGuid().ToString();
            Debug.Log("PlayFab: No user id found, added one: " + userId);
            PlayerPrefs.SetString("UserId", userId);
            PlayerPrefs.Save();
        }
        return userId;
    }

    void DoCustomLogin(Action<LoginResult> onsuccess, Action<PlayFabError> onError)
    {
        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = GetUserId(),
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, onsuccess, onError);
    }

    void DoAndroidLogin(Action<LoginResult> onsuccess, Action<PlayFabError> onError)
    {
        LoginWithAndroidDeviceIDRequest request = new LoginWithAndroidDeviceIDRequest
        {
            TitleId = PlayFabSettings.TitleId,
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
            OS = SystemInfo.operatingSystem,
            AndroidDevice = SystemInfo.deviceModel,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithAndroidDeviceID(request, onsuccess, onError);
    }

    public IEnumerator LogPlayerData(string key, string value)
    {
        yield return LogPlayerData(new Dictionary<string, string> { { key, value } });
    }

    public IEnumerator LogPlayerData(Dictionary<string, string> pairs)
    {
        UpdateUserDataRequest req = new UpdateUserDataRequest();
        {
            req.Data = pairs;
        };

        Action<Action<UpdateUserDataResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.UpdateUserData(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);
    }

    Dictionary<string, StatisticValue> stats = new Dictionary<string, StatisticValue>();
    [NonSerialized] public bool HasStatsFromServer = false;

    public bool TryGetStat(string key, out int value)
    {
        value = 0;
        StatisticValue stat;
        if (stats.TryGetValue(key, out stat))
        {
            value = stat.Value;
            return true;
        }
        return false;
    }

    public IEnumerator GetAllStats()
    {
        GetPlayerStatisticsRequest req = new GetPlayerStatisticsRequest { };
        Action<Action<GetPlayerStatisticsResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.GetPlayerStatistics(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);
        var result = (GetPlayerStatisticsResult)LastResult;
        if (result != null)
        {
            foreach (var stat in result.Statistics)
            {
                stats[stat.StatisticName] = stat;
            }
            HasStatsFromServer = true;
        }
    }

    public IEnumerator UpdateStat(string name, int value)
    {
        // Don't spam if client is not logged in/offline. Some score might also be posted before login completes and the SDK will then throw.
        if (!PlayFabClientAPI.IsClientLoggedIn())
            yield break;

        UpdatePlayerStatisticsRequest req = new UpdatePlayerStatisticsRequest();
        StatisticUpdate stat = new StatisticUpdate
        {
            Version = stats.ContainsKey(name) ? (uint?)stats[name].Version : null,
            StatisticName = name,
            Value = value,
        };
        req.Statistics = new List<StatisticUpdate> { stat };

        Action<Action<UpdatePlayerStatisticsResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.UpdatePlayerStatistics(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);
    }

    public IEnumerator GetAllPlayerData()
    {
        GetPlayerCombinedInfoRequest req = new GetPlayerCombinedInfoRequest
        {
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetPlayerStatistics = true,
                GetTitleData = true,
                GetUserData = true,
                GetUserInventory = true,
                GetUserReadOnlyData = true,
                GetUserVirtualCurrency = true
            }
        };

        Action<Action<GetPlayerCombinedInfoResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.GetPlayerCombinedInfo(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);
    }

    public IEnumerator DoLoginCo()
    {
        Action<Action<LoginResult>, Action<PlayFabError>> apiCall;

        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                apiCall = DoAndroidLogin;
                break;

            case RuntimePlatform.WebGLPlayer:
                apiCall = DoCustomLogin;
                break;

            default:
                apiCall = DoCustomLogin;
                break;
        }

        yield return ExecuteApiCallWithRetry(apiCall);
    }

    IEnumerator ExecuteApiCallWithRetry<TResult>(Action<Action<TResult>, Action<PlayFabError>> apiAction)
    {
        LastResult = null;

        float startTime = Time.time;
        float timeWaited = 0;
        int attempts = 0;
        TResult result = default(TResult);

        while (true)
        {
            attempts++;
            if (attempts > 5)
            {
                Debug.LogWarning("PlayFab: Cannot connect, giving up.");
                break;
            }

            bool callComplete = false;
            bool callSuccess = false;
            float apiCallRetryTime = 2.0f;

            Action<TResult> onSuccess = callResult =>
            {
                float timeTotal = Time.time - startTime;
                Debug.Log("PlayFab: Request succesful, ms = " + timeTotal);
                result = callResult;
                callComplete = true;
                callSuccess = true;
            };

            Action<PlayFabError> onError = error =>
            {
                string fullMsg = error.ErrorMessage;
                if (error.ErrorDetails != null)
                    foreach (var pair in error.ErrorDetails)
                        foreach (var eachMsg in pair.Value)
                            fullMsg += "\n" + pair.Key + ": " + eachMsg;

                Debug.LogError(fullMsg);
                callComplete = true;
            };

            Debug.Log("PlayFab: Sending request...");
            apiAction(onSuccess, onError);

            while (!callComplete)
            {
                yield return null;
                timeWaited = Time.time - startTime;
            }

            if (callSuccess)
                break;

            timeWaited = Time.time - startTime;

            // Don't spam, wait a bit.
            yield return new WaitForSeconds(apiCallRetryTime);
        }

        LastResult = result;
    }
}
