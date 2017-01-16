using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class EndMatchController : MonoBehaviour {

	public Text Message;
	public Text Score;
	public Button btnContinue;

	private DateTime lastCheckDate = DateTime.Now.AddMilliseconds(500);
	private bool updateWaitStatusComplete = false;
	public string gameScene = "Course";

	void Start() {

		GameSettings.NetworkPlayers.ForEach (x => x.Ready = false);

		btnContinue.gameObject.SetActive (false);

		Dictionary<int, string> scoring = new Dictionary<int, string> ();
		scoring.Add (1, "1st");
		scoring.Add (2, "2nd");
		scoring.Add (3, "3rd");
		scoring.Add (4, "4th");
		Score.text = string.Format ("You scored: {0}", scoring.ContainsKey (GameSettings.HoleStatus.playerRanking) ? 
			scoring[GameSettings.HoleStatus.playerRanking] : "Last");
	}


	public void OnClickContinue() {

		NetworkClientManager.NetworkPlayerReady (this, () => { GetNetworkPlayer(); });
		btnContinue.gameObject.SetActive (false);
	}

	void UpdatePlayerInfo() {

		string message = string.Empty;
		message = string.Join ("\r\n", GameSettings.NetworkPlayers.Select (x => string.Format ("{0} - {1}", x.accName, x.Ready ? "Ready" : "Waiting")).ToArray ());
		Message.text = message;

		if (!GameSettings.NetworkPlayers.Any (x => !x.Ready) && GameSettings.NetworkPlayers.Count > 1) {
			// load the seen
			GameSettings.HoleStatus.currentHoleIndex = 1;
			SceneManager.LoadScene(gameScene, LoadSceneMode.Single);

		}
	}

	// Update is called once per frame
	void Update () {
		if (DateTime.Now.Subtract (lastCheckDate).TotalMilliseconds > 1000) {
			GetNetworkPlayer ();
		}
	}

	void GetNetworkPlayer()
	{
		lastCheckDate = DateTime.Now;
		if (updateWaitStatusComplete) {
			NetworkClientManager.GetNetworkPlayers (this, () => UpdatePlayerInfo ());
		} else {
			updateWaitStatusComplete = true;
			//var playerData = GameSettings.NetworkPlayers.FirstOrDefault (x => x.objectId == NetworkClientManager.player.UID);
			GameSettings.NetworkPlayers.ForEach (x => {
				NetworkClientManager.SendGameWaitingData (x, this);
			});

			btnContinue.gameObject.SetActive (true);
		}

	}

}
