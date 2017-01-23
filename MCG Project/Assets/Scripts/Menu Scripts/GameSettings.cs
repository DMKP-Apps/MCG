using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerMode {
    Single,
    LocalMultiplayer,
    ServerMultiplayer
}

public class HoleStatus
{
	public int currentHoleIndex = 1;
	public string currentHoleName = "";
	public int playerRanking = 0;
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

	public static GameInfoResults GameInfo;
	public static bool isRace;

	private static HoleStatus _holeStatus = new HoleStatus();
	public static HoleStatus HoleStatus { get {  return _holeStatus; } set { _holeStatus = value; }}

	public static float? ShotPower;

    public static Vector3 EstimatedShotLocation;
    public static Vector3 CurrentCannonLocation;
    public static Vector3 EstimatedShotHighPoint;


}
