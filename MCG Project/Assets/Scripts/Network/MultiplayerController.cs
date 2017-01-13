using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class MultiplayerController : MonoBehaviour {

	public Text Message;

	private DateTime lastCheckDate = DateTime.Now;
	public string gameScene = "Course";

	void OnApplicationFocus( bool hasFocus )
	{
		if (!hasFocus && NetworkClientManager.IsOnline) {
			NetworkClientManager.Logoff ();
			SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
		}
	}

	// Use this for initialization
	void Start () {
		NetworkClientManager.NetworkPlayerReady (this, () => { });
	}

	void UpdatePlayerInfo() {

		string message = string.Empty;
		message = string.Join ("\r\n", GameSettings.NetworkPlayers.Select (x => string.Format ("{0} - {1}", x.accName, x.Ready ? "Ready" : "Waiting")).ToArray ());
		Message.text = message;

		if (!GameSettings.NetworkPlayers.Any (x => !x.Ready) && GameSettings.NetworkPlayers.Count > 1) {
			// load the seen
			SceneManager.LoadScene(gameScene, LoadSceneMode.Single);

		}
	}
	
	// Update is called once per frame
	void Update () {
		if (DateTime.Now.Subtract (lastCheckDate).TotalMilliseconds > 1000) {
			NetworkClientManager.GetNetworkPlayers (this, () => UpdatePlayerInfo());
		}
	}
}
