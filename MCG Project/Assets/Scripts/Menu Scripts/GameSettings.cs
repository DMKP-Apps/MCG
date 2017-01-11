using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerMode {
    Single,
    LocalMultiplayer,
    ServerMultiplayer
}

public static class GameSettings {

    public static PlayerMode playerMode = PlayerMode.Single;
    public static int LocalMultiplayerCount = 2;


}
