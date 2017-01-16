using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class NetworkMessage
{
    public string Key;
    public string Request;
    public string Data;
    public string Response;
    public bool IsComplete;
}

public class Player
{
    public string UID;
    public string AccountName;
}

public class NetworkController : MonoBehaviour {

    public string ServerUrl = "http://localhost:8321";

    private Player player = null;

    // Use this for initialization
    void Start () {

        player = new Player();
        player.UID = SystemInfo.deviceUniqueIdentifier;
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("accName"))) {
            player.AccountName = PlayerPrefs.GetString("accName");
        }

        //PlayerPrefs.SetString("UID", SystemInfo.deviceUniqueIdentifier);

        //Debug.Log(Application.platform);
        //Debug.Log(Application.persistentDataPath);
        //Debug.Log(SystemInfo.deviceName);
        //Debug.Log(SystemInfo.deviceType);
        //Debug.Log(SystemInfo.deviceUniqueIdentifier);

        //PlayerPrefs.SetString("deviceName", SystemInfo.deviceName);

        //PlayerPrefs.SetString("AccID", string.Empty);

        Login((data) => { }, (error) => { });



    }

    public void Login(Action<string> onComplete, Action<string> onError)
    {
        StartCoroutine(OnPostRequest("/Message/Request/Login", player, (data) => {
            player = JsonUtility.FromJson<Player>(data);
            PlayerPrefs.SetString("accName", player.AccountName);
            PlayerPrefs.Save();
            onComplete(data);
        }, onError));
    }

    public void CreateMessage(NetworkMessage message, Action<string> onComplete, Action<string> onError)
    {
        StartCoroutine(OnPostRequest("/Message/Request/Create", message, onComplete, onError));
    }
    
    
    public void Ping(Action<string> onComplete, Action<string> onError)
    {
        StartCoroutine(OnGetRequest("/Message/Request/Ping", onComplete, onError));
    }


    IEnumerator OnGetRequest(string methodUrl, Action<string> onComplete, Action<string> onError)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/{1}", ServerUrl.TrimEnd('/'), methodUrl.TrimStart('/'))))
        {
            yield return www.Send();

            if (www.isError)
            {
                onError(www.error);
            }
            else {
                // Show results as text
                onComplete(www.downloadHandler.text);
            }
        }
    }

    IEnumerator OnPostRequest(string methodUrl, object data, Action<string> onComplete, Action<string> onError)
    {
        //byte[] bytes = System.Text.Encoding.UTF8.GetBytes();
        string jsonData = JsonUtility.ToJson(data);
        using (UnityWebRequest www = new UnityWebRequest(string.Format("{0}/{1}", ServerUrl.TrimEnd('/'), methodUrl.TrimStart('/')), UnityWebRequest.kHttpVerbPOST))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            UploadHandlerRaw uH = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            uH.contentType = "application/json";
            www.uploadHandler = uH;

            DownloadHandler dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;

            yield return www.Send();

            if (www.isError)
            {
                onError(www.error);
            }
            else {
                // Show results as text
                onComplete(www.downloadHandler.text);
            }
        }
    }


}
