using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class EndMatchController : MonoBehaviour {

	public Text Score;
	

	private DateTime lastCheckDate = DateTime.Now.AddMilliseconds(500);
	public string gameScene = "Course";

    public List<PlayerTab> playerTabs;

	void Start() {

        List<RoomAttendee> players = new List<RoomAttendee>();
        if (playerTabs.Count == 4 && !playerTabs.Any(x => x == null))
        {
            if (GameSettings.GameInfo != null && GameSettings.GameInfo.Hole != null && GameSettings.Room != null)
            {
                if (GameSettings.GameInfo.Hole._1st != null)
                {
                    playerTabs[0].SetPlayerNumber(GameSettings.GameInfo.Hole._1st.playerNumber);
                    playerTabs[0].gameObject.SetActive(true);
                }
                else
                {
                    playerTabs[0].gameObject.SetActive(false);
                }
                if (GameSettings.GameInfo.Hole._2nd != null)
                {
                    playerTabs[1].SetPlayerNumber(GameSettings.GameInfo.Hole._2nd.playerNumber);
                    playerTabs[1].gameObject.SetActive(true);
                }
                else
                {
                    playerTabs[1].gameObject.SetActive(false);
                }

                if (GameSettings.GameInfo.Hole._3rd != null)
                {
                    playerTabs[2].SetPlayerNumber(GameSettings.GameInfo.Hole._3rd.playerNumber);
                    playerTabs[2].gameObject.SetActive(true);
                }
                else
                {
                    playerTabs[2].gameObject.SetActive(false);
                }
                if (GameSettings.GameInfo.Hole._4th != null)
                {
                    playerTabs[3].SetPlayerNumber(GameSettings.GameInfo.Hole._4th.playerNumber);
                    playerTabs[3].gameObject.SetActive(true);
                }
                else
                {
                    playerTabs[3].gameObject.SetActive(false);
                }
            }
        }

        Dictionary<int, string> scoring = new Dictionary<int, string>();
        scoring.Add(1, "1st");
        scoring.Add(2, "2nd");
        scoring.Add(3, "3rd");
        scoring.Add(4, "4th");
        Score.text = string.Format("You scored: {0}", scoring.ContainsKey(GameSettings.HoleStatus.playerRanking) ?
            scoring[GameSettings.HoleStatus.playerRanking] : "Last");
    }

	// Update is called once per frame
	void Update () {
		if (DateTime.Now.Subtract (lastCheckDate).TotalMilliseconds > 1000) {
			GetNetworkPlayer ();
		}
	}

    void UpdatePlayerInfo()
    {

        if (GameSettings.Room == null)
        {
            return;
        }

        //Title.text = GameSettings.Room.status == RoomStatus.New ? "Locating Players..." : GameSettings.Room.status == RoomStatus.Waiting ? "Loading Course..."
        //        : GameSettings.Room.status == RoomStatus.InProgress ? "Loading..." : GameSettings.Room.status == RoomStatus.Closed ? "Closed" : "...";

        //string message = string.Empty;
        //message = string.Join("\r\n", GameSettings.Room.attendees.OrderBy(x => x.position).Select(x => string.Format("{0} - {1}", x.AccountName, !x.Removed ? "Ready" : "Removed")).ToArray());
        //Message.text = message;

        if (GameSettings.Room.status == RoomStatus.InProgress)
        {   // hole is now ready to go...
            GameSettings.HoleStatus.currentHoleIndex = GameSettings.Room.currentHole;
            SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
        else if (GameSettings.Room.status == RoomStatus.Closed)
        {   // TODO... open the game closed
            NetworkClientManager.Logoff();
            SceneManager.LoadScene("GameClosed", LoadSceneMode.Single);
        }
        
    }

    void GetNetworkPlayer()
    {
        lastCheckDate = DateTime.Now;
        NetworkClientManager.GetRoomStatus(this, () => UpdatePlayerInfo(), (data) => {
            NetworkClientManager.Logoff();
            SceneManager.LoadScene("GameClosed", LoadSceneMode.Single);
        });
        //NetworkClientManager.GetNetworkPlayers (this, () => UpdatePlayerInfo());
    }


}
