using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkClientController : MonoBehaviour
{

}

public static class NetworkClientManager
{
    public static string ServerUrl = "http://localhost:8321";
    //public static string ServerUrl = "http://mcg.moebull.com/";
    public static NetworkPlayerInfo player = null;
	public static bool IsOnline { get { return GameSettings.playerMode == PlayerMode.ServerMultiplayer; } 
		private set { GameSettings.playerMode = value ? PlayerMode.ServerMultiplayer : PlayerMode.Single; } }

    public static void Logoff() {
		if (player == null) {
			return;
		}
        IsOnline = false;
		GameSettings.NetworkPlayers = null;
		var data = OnGetRequest<bool>(string.Format("/Message/Request/Logout?id={0}", player.UID));
		if (data.Status)
		{
			IsOnline = true;
		}
		else {
			IsOnline = false;
		}
    }

	public static void Login(bool isRace = false)
    {
        GameSettings.NetworkPlayers = null;
        IsOnline = false;

        player = new NetworkPlayerInfo();
		player.isRace = isRace;
        player.UID = SystemInfo.deviceUniqueIdentifier;
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("accName")))
        {
            player.AccountName = PlayerPrefs.GetString("accName");
        }

        var data = OnPostRequest<NetworkPlayerInfo>("/Message/Request/Login", player);
        if (data.Status)
        {
            player = data.Data;
            PlayerPrefs.SetString("accName", player.AccountName);
            PlayerPrefs.Save();
            IsOnline = true;
			GameSettings.isRace = isRace;
        }
        else {
            IsOnline = false;
        }
    }

    public static void SendGameObjectData(NetworkObjectData objectData, MonoBehaviour gameObject = null, Action<string> onComplete = null, Action<string> onError = null) {

		objectData.sessionId = GameSettings.SessionId;
        objectData.objectId = player.UID;
        objectData.accName = player.AccountName;

        if (gameObject != null)
        {
			gameObject.StartCoroutine(OnPostRequest("/Message/Request/SaveObjectData", objectData, (data) => { 
				Debug.Log(data);
			}, (err) => {
				Debug.Log(err);
			}));
        }
        else
        {
            var data = OnPostRequest<string>("/Message/Request/SaveObjectData", objectData);
        }
    }

	public static void SendGameWaitingData(NetworkPlayerData objectData, MonoBehaviour gameObject = null, Action<string> onComplete = null, Action<string> onError = null) {

		objectData.sessionId = GameSettings.SessionId;
		objectData.objectId = player.UID;
		objectData.accName = player.AccountName;

		if (gameObject != null)
		{
			gameObject.StartCoroutine(OnPostRequest("/Message/Request/SaveWaitingData", objectData, (data) => { 
				Debug.Log(data);
			}, (err) => {
				Debug.Log(err);
			}));
		}
		else
		{
			var data = OnPostRequest<string>("/Message/Request/SaveObjectData", objectData);
		}
	}

	[Serializable]
	public class GetAvailablePlayersResult
	{
		public NetworkPlayerData[] Items;
	}



	public static void GetNetworkPlayers(MonoBehaviour gameObject, Action onComplete, Action<string> onError = null)
	{

		if (gameObject != null)
		{
			gameObject.StartCoroutine(OnGetRequest(string.Format("/Message/Request/GetNetworkPlayers?id={0}", player.UID), (data) => {
				data = string.Format("{{ \"Items\": {0} }}", data);
				var objectData = JsonUtility.FromJson<GetAvailablePlayersResult>(data);
				GameSettings.NetworkPlayers = objectData.Items.ToList();
				onComplete();
			}, onError));
		}

	}

	public static void GetGameInfo(MonoBehaviour gameObject, Action onComplete, Action<string> onError = null)
	{

		if (gameObject != null)
		{
			gameObject.StartCoroutine(OnGetRequest(string.Format("/Message/Request/GetGameInfo?id={0}", player.UID), (data) => {
				try { 
					data = string.Format("{{ \"Items\": {0} }}", data);
					var objectData = JsonUtility.FromJson<GameInfoResults>(data);
					GameSettings.GameInfo = objectData;
					onComplete();
				}
				catch {
					// do nothing
					Debug.Log(data);
				}
			}, onError));
		}

	}

	public static void IsPlayerOnline(MonoBehaviour gameObject, Action<bool> onComplete, Action<string> onError = null)
	{

		if (gameObject != null)
		{
			gameObject.StartCoroutine(OnGetRequest(string.Format("/Message/Request/IsPlayerOnline?id={0}", player.UID), (data) => {
				var objectData = JsonUtility.FromJson<NetworkPlayerData>(data);
				onComplete(objectData != null);
			}, onError));
		}

	}


	public static void NetworkPlayerReady(MonoBehaviour gameObject, Action onComplete, Action<string> onError = null)
	{

		if (gameObject != null)
		{
			gameObject.StartCoroutine(OnGetRequest(string.Format("/Message/Request/NetworkPlayerReady?id={0}", player.UID), (data) => {
				var objectData = data;
				GameSettings.SessionId = objectData;
				onComplete();
			}, onError));
		}

	}

    public static void GetGameObjectData(string objectId, MonoBehaviour gameObject, Action<NetworkObjectData> onComplete, Action<string> onError = null)
    {

        if (gameObject != null)
        {
            gameObject.StartCoroutine(OnGetRequest(string.Format("/Message/Request/GetById?id={0}", objectId), (data) => {
                var objectData = JsonUtility.FromJson<NetworkObjectData>(data);
				if(objectData.type == "NetworkObjectData") {
					onComplete(objectData);
				}
                
            }, onError));
        }
        
    }


    public static string Ping()
    {
        var data = OnGetRequest<string>("/Message/Request/Ping");
        if (data.Status)
        {
            return data.Data;
        }
        else {
            return data.Message;
        }
    }
		 

    private static int timeoutMilliseconds = 20000;

    static ResponseData<string> OnGetRequest(string methodUrl)
    {
        return OnGetRequest<string>(methodUrl);
    }
    static ResponseData<TData> OnGetRequest<TData>(string methodUrl)
    {
		        using (UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/{1}", ServerUrl.TrimEnd('/'), methodUrl.TrimStart('/'))))
        {
            var response = www.Send();
            
            DateTime startTime = DateTime.Now;
            while (!response.isDone && DateTime.Now.Subtract(startTime).TotalMilliseconds < timeoutMilliseconds)
            {
                Debug.Log(string.Format("Progress: {0}", response.progress));
            }

            if (!response.isDone)
            {
                return new ResponseData<TData>()
                {
                    Status = false,
                    Message = "Timed out.",
                    Data = default(TData)
                };
            }
            else if (www.isError)
            {
                return new ResponseData<TData>()
                {
                    Status = false,
                    Message = www.error,
                    Data = default(TData)
                };
            }
            else {
                // Show results as text
                return new ResponseData<TData>()
                {
                    Status = true,
                    Message = "Request completed successfully",
					Data = typeof(TData) == typeof(bool) ? (TData)(bool.Parse(www.downloadHandler.text.ToLower()) as object) : 
						typeof(TData) == typeof(string) ? (TData)(www.downloadHandler.text as object) : 
						JsonUtility.FromJson<TData>(www.downloadHandler.text)
                };
            }
        }
    }

    static IEnumerator OnGetRequest(string methodUrl, Action<string> onComplete, Action<string> onError)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/{1}", ServerUrl.TrimEnd('/'), methodUrl.TrimStart('/'))))
        {
            yield return www.Send();

            if (www.isError)
            {
                if (onError != null)
                {
                    onError(www.error);
                }
            }
            else {
                if (onComplete != null)
                {
                    // Show results as text
                    onComplete(www.downloadHandler.text);
                }

            }
        }
    }


    static ResponseData<string> OnPostRequest(string methodUrl, object data)
    {
        return OnPostRequest<string>(methodUrl, data);
    }
    static ResponseData<TData> OnPostRequest<TData>(string methodUrl, object data)
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

            var response = www.Send();

            DateTime startTime = DateTime.Now;
            while (!response.isDone && DateTime.Now.Subtract(startTime).TotalMilliseconds < timeoutMilliseconds) {
                Debug.Log(string.Format("Progress: {0}", response.progress));
            }
            
            if (!response.isDone)
            {
                return new ResponseData<TData>()
                {
                    Status = false,
                    Message = "Timed out.",
                    Data = default(TData)
                };
            }
            else if (www.isError)
            {
                return new ResponseData<TData>() {
                    Status = false,
                    Message = www.error,
                    Data = default(TData)
                };
            }
            else {
                // Show results as text
                return new ResponseData<TData>()
                {
                    Status = true,
                    Message = "Request completed successfully",
					Data = typeof(TData) == typeof(bool) ? (TData)(www.downloadHandler.text as object) : 
						typeof(TData) == typeof(string) ? (TData)(www.downloadHandler.text as object) : 
						JsonUtility.FromJson<TData>(www.downloadHandler.text)
                };
            }
        }
    }

    static IEnumerator OnPostRequest(string methodUrl, object data, Action<string> onComplete, Action<string> onError)
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
                if (onError != null)
                {
                    onError(www.error);
                }
            }
            else {
                if (onComplete != null)
                {
                    // Show results as text
                    onComplete(www.downloadHandler.text);
                }
                    
            }
        }
    }

}


public class ResponseData<T>
{
    public bool Status { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
}
