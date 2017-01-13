﻿using System.Collections;
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

	private static List<NetworkPlayerData> _networkPlayers = new List<NetworkPlayerData> ();

	public static string SessionId = string.Empty;

	public static List<NetworkPlayerData> NetworkPlayers{
		get { 
			if (playerMode == PlayerMode.ServerMultiplayer && _networkPlayers != null) {
				return _networkPlayers;
			} else {
				return new List<NetworkPlayerData> ();
			}
		}
		set { 
			_networkPlayers = value;
		}
	}
}
