using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClosedController : MonoBehaviour {


    public void OnOkClick() {
        NetworkClientManager.Logoff();
        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
    }
}
