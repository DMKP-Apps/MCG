using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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

    public static Material CannonBarrelMaterial;
    public static Material CannonWheelMaterial;

	//public static List<NetworkPlayerData> NetworkPlayers{
	//	get { 
	//		if (playerMode == PlayerMode.ServerMultiplayer && Room != null) {// _networkPlayers != null) {
	//			return _networkPlayers;
	//		} else {
	//			return new List<NetworkPlayerData> ();
	//		}
	//	}
	//	set { 
	//		_networkPlayers = value;
	//	}
	//}

	public static GameInfoResults GameInfo;
	public static bool isRace;

	private static HoleStatus _holeStatus = new HoleStatus();
	public static HoleStatus HoleStatus { get {  return _holeStatus; } set { _holeStatus = value; }}

	public static float? ShotPower;

    public static Vector3 EstimatedShotLocation;
    public static Vector3 CurrentCannonLocation;
    public static Vector3 EstimatedShotHighPoint;

    public static roomInfo Room;

    public static UserPreferences preferences = new UserPreferences();


}

public class UserPreferences
{
    private void load()
    {
        isInitialized = true;
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("item1")))
        {
            var values = PlayerPrefs.GetString("item1").Split(',').Select(x =>
            {
                int i = 0;
                if (!int.TryParse(x.Trim(), out i))
                {
                    return null;
                }
                return new Nullable<int>(i);
            }).Where(x => x.HasValue).Select(x => x.Value).ToList();

            if (values.Count > 0)
            {
                coins = values[0];
            }

        }
    }

    private int _coins = 0;
    public int coins
    {
        get
        {
            if (!isInitialized) {
                load();
            }
            return _coins;
        }
        set
        {
            _coins = value;
        }
    }

    private bool isInitialized = false;

    public void Save()
    {
        var int_items = string.Join(",", new string[] { coins.ToString() });
        PlayerPrefs.SetString("item1", int_items);
        PlayerPrefs.Save();

    }
}
