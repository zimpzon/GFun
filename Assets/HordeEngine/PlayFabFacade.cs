using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;

/// <summary>
/// PlayerData: key/value where value is a string. Could be JSON.
/// PlayerStatistics: key/value where value is an int. PlayFab always creates a leaderboard for these. Min, Max, Sum can be set in PlayFab.com.
/// </summary>
public class PlayFabFacade : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern string GetURLFromPage();

    public static PlayFabFacade Instance;
    public static GetPlayerStatisticsResult AllStats = new GetPlayerStatisticsResult();
    public static GetPlayerCombinedInfoResult AllData = new GetPlayerCombinedInfoResult();

    public bool LoginWhenInEditor = false;

    [NonSerialized] public object LastResult;
    [NonSerialized] public string LastError;
    [NonSerialized] public bool LoginProcessComplete;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Application.isEditor && !LoginWhenInEditor)
        {
            LoginProcessComplete = true;
            return;
        }

        StartCoroutine(InitializePlayFab());
    }

    public IEnumerator InitializePlayFab()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            LoginProcessComplete = true;
            yield break; // Already logged in
        }

        Debug.Log("PlayFab: Login...");
        yield return DoLoginCo();
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            LoginProcessComplete = true;
            yield break;
        }

        Debug.Log("PlayFab: Get all stats...");
        yield return GetAllStats();
        AllStats = (GetPlayerStatisticsResult)LastResult;

        Debug.Log("PlayFab: Get all data...");
        yield return GetAllPlayerData();
        AllData = (GetPlayerCombinedInfoResult)LastResult;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("PlayFab: Sending WebGL url info...");
            yield return LogPlayerData(
                new Dictionary<string, string> {
            { "hosting_url", Application.absoluteURL },
            { "page_top_url", GetURLFromPage() }
            });
        }

        LoginProcessComplete = true;
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

    public void LogPlayerDataAsync(string key, string value, UserDataPermission permission = UserDataPermission.Private)
    {
        StartCoroutine(LogPlayerData(new Dictionary<string, string> { { key, value } }));
    }

    public IEnumerator LogPlayerData(Dictionary<string, string> pairs, UserDataPermission permission = UserDataPermission.Private)
    {
        UpdateUserDataRequest req = new UpdateUserDataRequest();
        {
            req.Data = pairs;
            req.Permission = permission;
        };

        Action<Action<UpdateUserDataResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.UpdateUserData(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);
    }

    public void UpdatePlayerNameAsync(string name, Action callback)
    {
        StartCoroutine(UpdatePlayerName(name, callback));
    }

    public IEnumerator UpdatePlayerName(string name, Action callback)
    {
        var req = new UpdateUserTitleDisplayNameRequest();
        {
            req.DisplayName = name;
        };

        Action<Action<UpdateUserTitleDisplayNameResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.UpdateUserTitleDisplayName(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);

        callback();
    }

    Dictionary<string, StatisticValue> CurrentStats = new Dictionary<string, StatisticValue>();
    [NonSerialized] public bool HasStatsFromServer = false;

    public bool TryGetStat(string key, out int value)
    {
        value = 0;
        StatisticValue stat;
        if (CurrentStats.TryGetValue(key, out stat))
        {
            value = stat.Value;
            return true;
        }
        return false;
    }

    public IEnumerator GetLeaderboardAroundPlayer(string name, int max)
    {
        GetLeaderboardAroundPlayerRequest req = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = name,
            MaxResultsCount = max,
            ProfileConstraints = new PlayerProfileViewConstraints { ShowDisplayName = true },
        };
        Action<Action<GetLeaderboardAroundPlayerResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.GetLeaderboardAroundPlayer(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);
    }

    public IEnumerator GetLeaderboard(string name, int start, int max)
    {
        GetLeaderboardRequest req = new GetLeaderboardRequest
        {
            StatisticName = name,
            MaxResultsCount = max,
            StartPosition = start,
            ProfileConstraints = new PlayerProfileViewConstraints { ShowDisplayName = true },
        };
        Action<Action<GetLeaderboardResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.GetLeaderboard(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);
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
                CurrentStats[stat.StatisticName] = stat;
            }
            HasStatsFromServer = true;
        }
    }

    public IEnumerator UpdateStats(List<(string name, int value)> newStats)
    {
        // Don't spam if client is not logged in/offline. Some score might also be posted before login completes and the SDK will then throw.
        if (!PlayFabClientAPI.IsClientLoggedIn())
            yield break;

        var req = new UpdatePlayerStatisticsRequest();
        req.Statistics = new List<StatisticUpdate>();
        foreach (var newStat in newStats)
        {
            var stat = new StatisticUpdate
            {
                Version = CurrentStats.ContainsKey(newStat.name) ? (uint?)CurrentStats[newStat.name].Version : null,
                StatisticName = newStat.name,
                Value = newStat.value,
            };
            req.Statistics.Add(stat);
        }

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
        LastError = null;

        string name = typeof(TResult).Name;
        float startTime = Time.realtimeSinceStartup;
        float timeWaited = 0;
        TResult result = default;

        bool callComplete = false;

        Action<TResult> onSuccess = callResult =>
        {
            float timeTotal = Time.realtimeSinceStartup - startTime;
            Debug.Log($"PlayFab: Request succesful ({name}), ms = " + timeTotal * 1000);
            result = callResult;
            callComplete = true;
        };

        Action<PlayFabError> onError = error =>
        {
            string fullMsg = error.ErrorMessage;
            if (error.ErrorDetails != null)
                foreach (var pair in error.ErrorDetails)
                    foreach (var eachMsg in pair.Value)
                        fullMsg += "\n" + pair.Key + ": " + eachMsg;

            Debug.LogError($"PlayFab: Request ({name}) failed: {fullMsg}");
            LastError = fullMsg;
            callComplete = true;
        };

        Debug.Log($"PlayFab: Sending request...{name}");
        apiAction(onSuccess, onError);

        while (!callComplete)
        {
            yield return null;
            timeWaited = Time.realtimeSinceStartup - startTime;
        }

        timeWaited = Time.realtimeSinceStartup - startTime;
        LastResult = result;
    }
}
