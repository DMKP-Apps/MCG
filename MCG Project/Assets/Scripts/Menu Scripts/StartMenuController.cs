using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour {


    public string gameScene = "Course";
    
    
    public void OnLoadGameSceneClick(string mode) {

        GameSettings.playerMode = mode == "servermulti" ? PlayerMode.ServerMultiplayer : mode == "localmulti" ? PlayerMode.LocalMultiplayer : PlayerMode.Single;
        if (GameSettings.playerMode == PlayerMode.ServerMultiplayer)
        {
            NetworkClientManager.Login();
        }
        else {
            NetworkClientManager.Logoff();
        }


        SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }
}
