using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketClient.Library;

public class WebSocketController : MonoBehaviour {

    // Use this for initialization
    private Client client;
    void Start () {
        StartCoroutine(OnStartClient());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator OnStartClient()
    {
        client = new Client("a89d0a4eb26642f899fb7d63aac69147");//Guid.NewGuid().ToString("N"));
        client.Name = "Test";
        // on load attempt to connect to service
        Debug.Log("Attempting to join remote room...");

        yield return true;

        var status = client.StartClient("Localhost",
            11100,
            (data) =>
            {
                if (!data.Status) WriteError(data.Message);
                else OnServerMessage(data);
            });

        client.SendRequest<string>(client.ID, "Logon", "Game");

        
    }

    void WriteError(string message)
    {
        Debug.Log(string.Format("ERROR: {0}", message));
    }

    void WriteMessage(string message)
    {
        Debug.Log(message);
    }

    void OnServerMessage(SocketClient.Library.Responses.SocketResponse responseObj)
    {
        var response = responseObj.Data;
        var actionKey = string.Format("{0}.{1}", responseObj.Service, responseObj.Method).ToLower();

        WriteMessage(response.ToString());

        //Dictionary<string, Action<object>> handleRequestActions = new Dictionary<string, Action<object>>();
        //handleRequestActions.Add("Game.Spin", (data) => {


        //});

        //Dictionary<string, Action<object>> actions = new Dictionary<string, Action<object>>();

        //actions.Add("Game.Logon".ToLower(), (data) =>
        //{
        //    var obj = data.ToJsonString().FromJsonString<string>();
        //    client.Name = obj;
        //    client.SendRequest<string>(client.Name, "GetProfile", "Profile");
        //    client.SendRequest<string>(client.Name, "Join", "Game");
        //});


        //if (actions.ContainsKey(actionKey))
        //    actions[actionKey](response);


    }
}
