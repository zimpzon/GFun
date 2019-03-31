//using UnityEngine;

//public class KongregateApi : MonoBehaviour
//{
//    public static KongregateApi Instance;

//    public int UserId;
//    public string UserName;
//    public string AuthToken;
//    public bool LoggedIn;

//    public void SubmitStat(string stat, int value)
//    {
//        Application.ExternalCall("kongregate.stats.submit", stat, value);
//    }

//    public void Start()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else if (Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Object.DontDestroyOnLoad(gameObject);
//        gameObject.name = "KongregateAPI";

//        Application.ExternalEval(
//          @"if(typeof(kongregateUnitySupport) != 'undefined'){
//        kongregateUnitySupport.initAPI('KongregateAPI', 'OnKongregateAPILoaded');
//      };"
//        );
//    }

//    public void OnKongregateAPILoaded(string userInfoString)
//    {
//        OnKongregateUserInfo(userInfoString);

//        Application.ExternalEval(@"
//      kongregate.services.addEventListener('login', function(){
//        var unityObject = kongregateUnitySupport.getUnityObject();
//        var services = kongregate.services;
//        var params=[services.getUserId(), services.getUsername(), 
//                    services.getGameAuthToken()].join('|');

//        unityObject.SendMessage('KongregateApi', 'OnKongregateUserInfo', params);
//    });");
//    }

//    public void OnKongregateUserInfo(string userInfoString)
//    {
//        if (string.IsNullOrEmpty(userInfoString))
//        {
//            Debug.Log("Kongregate User Info: nope");
//            StartCoroutine(Server.Instance.LogPlayerData("kongregate", "unsuccessful"));
//            return;
//        }

//        var info = userInfoString.Split('|');
//        UserId = System.Convert.ToInt32(info[0]);
//        UserName = info[1];
//        AuthToken = info[2];
//        Debug.Log("Kongregate User Info: " + UserName + ", userId: " + UserId);
//        LoggedIn = true;

//        GameManager.Instance.OnKongregateLogin();
//    }
//}
