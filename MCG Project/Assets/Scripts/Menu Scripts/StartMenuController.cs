using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour {


    public string gameScene = "Course";
	public string multiplayerScene = "MultiPlayerSetup";

    private bool is1PlayerKey = false;
    private bool is2PlayerKey = false;

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) && !is1PlayerKey && !is2PlayerKey)
        {
            is1PlayerKey = true;
            OnLoadGameSceneClick("single");
        }
        else if ((Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1)))
        {
            is1PlayerKey = false;
        }

        if ((Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) && !is1PlayerKey && !is2PlayerKey)
        {
            is2PlayerKey = true;
            OnLoadGameSceneClick("localmulti-2");
        }
        else if ((Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2)))
        {
            is2PlayerKey = false;
        }
    }


    public void OnLoadGameSceneClick(string mode) {

		GameSettings.playerMode = mode.StartsWith("servermulti") ? PlayerMode.ServerMultiplayer : mode.StartsWith("localmulti") ? PlayerMode.LocalMultiplayer : PlayerMode.Single;
		if (GameSettings.playerMode == PlayerMode.ServerMultiplayer)
        {
			NetworkClientManager.Login(mode.EndsWith("israce"));
			SceneManager.LoadScene(multiplayerScene, LoadSceneMode.Single);
        }
        else {
			if (GameSettings.playerMode == PlayerMode.LocalMultiplayer) {
				var players = mode.Substring ("localmulti".Length).TrimStart ('-', ' ').TrimEnd ('-', ' ');
				if (!int.TryParse (players, out GameSettings.LocalMultiplayerCount)) {
					GameSettings.LocalMultiplayerCount = 2;
				}	
			}
            NetworkClientManager.Logoff();
            //SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
            SceneManager.LoadScene("SelectSkin", LoadSceneMode.Single);
        }


        
    }
}
