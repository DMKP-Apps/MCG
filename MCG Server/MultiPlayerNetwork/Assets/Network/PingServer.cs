using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingServer : MonoBehaviour {

    private NetworkController networkController;

    private System.DateTime lastPing = System.DateTime.Now;
    public float waitInterval = 0f;

    // Use this for initialization
    void Start () {
        networkController = GameObject.Find("NetworkController").GetComponent<NetworkController>();

    }
	
	// Update is called once per frame
	void Update () {
        if (waitInterval > 0f && System.DateTime.Now.Subtract(lastPing).TotalMilliseconds > waitInterval) {
            lastPing = System.DateTime.Now;
            networkController.Ping((data) => { Debug.Log(string.Format("SUCCESS: {0}", data)); }, (error) => { Debug.Log(string.Format("ERROR: {0}", error)); });

            networkController.CreateMessage(new NetworkMessage {
                Request = "Request: " + System.DateTime.Now.ToString(),
                Data = "Data!!",
            }, (data) => { Debug.Log(string.Format("SUCCESS: {0}", data)); }, (error) => { Debug.Log(string.Format("ERROR: {0}", error)); });
        }
	}
}
