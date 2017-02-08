using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class MultiplayerController : MonoBehaviour {

	public Text Message;
    public Text Title;
    public Text Info;

    private DateTime lastCheckDate = DateTime.Now;
	public string gameScene = "Course";

	void OnApplicationFocus( bool hasFocus )
	{
		if (1 ==2 && !hasFocus && NetworkClientManager.IsOnline) {
			NetworkClientManager.Logoff ();
			SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
		}
	}

	// Use this for initialization
	void Start () {
		NetworkClientManager.NetworkPlayerReady (this, () => { });
		GameSettings.HoleStatus = new HoleStatus ();
	}

	void UpdatePlayerInfo() {

        if (GameSettings.Room == null)
        {
            return;
        }

        Title.text = GameSettings.Room.status == RoomStatus.New ? "Locating Players..." : GameSettings.Room.status == RoomStatus.Waiting ? "Loading Course..."
                : GameSettings.Room.status == RoomStatus.InProgress ? "Loading..." : GameSettings.Room.status == RoomStatus.Closed ? "Closed" : "...";

        string message = string.Empty;
        message = string.Join ("\r\n", GameSettings.Room.attendees.OrderBy(x => x.position).Select (x => string.Format ("{0} - {1}", x.AccountName, !x.Removed ? "Ready" : "Removed")).ToArray ());
        Message.text = message;

        if (GameSettings.Room.status == RoomStatus.InProgress)
        {
            lastCheckDate = DateTime.MaxValue;
            GameSettings.HoleStatus.currentHoleIndex = GameSettings.Room.currentHole;
            SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
        else if (GameSettings.Room.status == RoomStatus.Closed)
        {
            NetworkClientManager.Logoff();
            SceneManager.LoadScene("GameClosed", LoadSceneMode.Single);
        }
        else if (GameSettings.Room.status == RoomStatus.Waiting)
        {
            //var seconds = DateTime.Now.AddMilliseconds(GameSettings.Room.nextPhaseOn).Subtract(DateTime.Now).TotalSeconds;
            Info.text = string.Format("Game starting in {0} second(s).", Math.Floor(GameSettings.Room.nextPhaseOn / 1000));
        }
        else if (GameSettings.Room.status == RoomStatus.New)
        {
            Info.text = string.Format("{0} of {1} found", GameSettings.Room.getActiveAttendees().Count, GameSettings.Room.maxAttendance);
        }


        //if (!GameSettings.NetworkPlayers.Any (x => !x.Ready) && GameSettings.NetworkPlayers.Count > 1) {
        //	// load the seen


        //}
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
        NetworkClientManager.GetRoomStatus(this, () => UpdatePlayerInfo(), (data) => {
            NetworkClientManager.Logoff();
            SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
        });
        //NetworkClientManager.GetNetworkPlayers (this, () => UpdatePlayerInfo());
    }
}
