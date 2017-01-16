using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour {


    public string gameScene = "Course";
	public string multiplayerScene = "MultiPlayerSetup";
  
    
    public void OnLoadGameSceneClick(string mode) {

		GameSettings.playerMode = mode.StartsWith("servermulti") ? PlayerMode.ServerMultiplayer : mode == "localmulti" ? PlayerMode.LocalMultiplayer : PlayerMode.Single;
        if (GameSettings.playerMode == PlayerMode.ServerMultiplayer)
        {
			NetworkClientManager.Login(mode.EndsWith("israce"));
			SceneManager.LoadScene(multiplayerScene, LoadSceneMode.Single);
        }
        else {
            NetworkClientManager.Logoff();
			SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }


        
    }
}
