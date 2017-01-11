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
    public static string ServerUrl = "http://localhost:8321";//"http://mcg.moebull.com/";
    public static NetworkPlayerInfo player = null;
    public static bool IsOnline { get; private set; }

    private static Guid _sessionId = Guid.NewGuid();

    public static void Logoff() {
        IsOnline = false;
    }

    public static void Login()
    {
        _sessionId = Guid.NewGuid();
        IsOnline = false;

        player = new NetworkPlayerInfo();
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
        }
        else {
            IsOnline = false;
        }
    }

    public static void SendGameObjectData(GameObject gameObject, string holeId) {
        if (!IsOnline)
        {
            return;
        }

        NetworkObjectData objData = GetDataObject(gameObject, holeId);
        //var thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((args) => {
            var data = OnPostRequest<string>("/Message/Request/SaveObjectData", objData);
            if (!data.Status)
            {
                Debug.Log(data.Message);
            }
        //}));
        //thread.Start(objData);
        
                
    }

    public static void SendGameActionData(GameObject gameObject, string holeId, float torque, float turn, float power)
    {
        if (!IsOnline)
        {
            return;
        }

        NetworkActionShotData objData = GetDataObject<NetworkActionShotData>(gameObject, holeId);
        objData.torque = torque;
        objData.turn = turn;
        objData.power = power;

        //var thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((args) => {
            var data = OnPostRequest<string>("/Message/Request/SaveActionData", objData);
            if (!data.Status)
            {
                Debug.Log(data.Message);
            }
        //}));
        //thread.Start(objData);
    }

    private static NetworkObjectData GetDataObject(GameObject gameObject, string holeId)
    {
        return GetDataObject<NetworkObjectData>(gameObject, holeId);
    }
    private static TDataObject GetDataObject<TDataObject>(GameObject gameObject, string holeId)
        where TDataObject : NetworkObjectData, new()
    {
        return new TDataObject()
        {
            holeId = holeId,
            objectId = player.UID,
            objectName = gameObject.name,
            position_x = gameObject.transform.position.x,
            position_y = gameObject.transform.position.y,
            position_z = gameObject.transform.position.z,
            rotation_x = gameObject.transform.rotation.eulerAngles.x,
            rotation_y = gameObject.transform.rotation.eulerAngles.y,
            rotation_z = gameObject.transform.rotation.eulerAngles.z,
            sessionId = _sessionId.ToString()
        };
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
                    Data = typeof(TData) == typeof(string) ? (TData)(www.downloadHandler.text as object) : JsonUtility.FromJson<TData>(www.downloadHandler.text)
                };
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
                    Data = typeof(TData) == typeof(string) ? (TData)(www.downloadHandler.text as object) : JsonUtility.FromJson<TData>(www.downloadHandler.text)
                };
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
