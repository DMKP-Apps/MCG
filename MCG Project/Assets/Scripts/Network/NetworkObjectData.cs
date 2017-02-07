using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class NetworkPlayerData
{
	public NetworkPlayerData() {
		timeStamp = DateTime.Now.ToUniversalTime();
		uniqueId = Guid.NewGuid ().ToString ();
	}

	public string type;
	public DateTime timeStamp;
	public string objectId;
	public string accName;
	public string sessionId;
	public string objectName;
	public bool Ready;
	public string uniqueId;
	public bool isRace;

}


[Serializable]
public class NetworkObjectData
{
    public NetworkObjectData() {
        timeStamp = DateTime.Now.ToUniversalTime();
        fire = false;
		playerType = "default";
		uniqueId = Guid.NewGuid ().ToString ();
		Ready = false;
		isRace = GameSettings.isRace;
    }

	public string type;
	public string uniqueId;
    public DateTime timeStamp;
    public string objectId;
    public string accName;
    public string sessionId;
    public string objectName;
    public string holeId;

    public float root_position_x;
    public float root_position_y;
    public float root_position_z;

    public float root_rotation_x;
    public float root_rotation_y;
    public float root_rotation_z;

    public float cannon_position_x;
    public float cannon_position_y;
    public float cannon_position_z;

    public float cannon_rotation_x;
    public float cannon_rotation_y;
    public float cannon_rotation_z;

    public bool fire;
    public float fire_torque;
    public float fire_turn;
    public float fire_power;
    public int currentBullet;
	public string playerType;
	public float fire_accurracy;
	public bool isRace;
	public bool holeComplete;
	public int stroke;
	
	public bool Ready;
	
	public double waitMilliseconds;

    public bool Compare(NetworkObjectData other)
    {
        if (other == null) {
            return false;
        }

		return this.holeId == other.holeId &&
			this.stroke == other.stroke &&

            this.root_position_x == other.root_position_x &&
            this.root_position_y == other.root_position_y &&
            this.root_position_z == other.root_position_z &&

            this.root_rotation_x == other.root_rotation_x &&
            this.root_rotation_y == other.root_rotation_y &&
            this.root_rotation_z == other.root_rotation_z &&

            this.cannon_position_x == other.cannon_position_x &&
            this.cannon_position_y == other.cannon_position_y &&
            this.cannon_position_z == other.cannon_position_z &&

            this.cannon_rotation_x == other.cannon_rotation_x &&
            this.cannon_rotation_y == other.cannon_rotation_y &&
            this.cannon_rotation_z == other.cannon_rotation_z &&

            this.fire == other.fire &&
            this.fire_torque == other.fire_torque &&
            this.fire_turn == other.fire_turn &&
            this.fire_power == other.fire_power &&
			this.fire_accurracy == other.fire_accurracy &&

			this.holeComplete == other.holeComplete &&
            this.currentBullet == other.currentBullet;
    }

}

[Serializable]
public class PositionStatusResults
{
    public string UID;
    public int playerNumber;
}

[Serializable]
public class HoleStatusResults
{
    public RoomStatus status;
    public PositionStatusResults _1st;
    public PositionStatusResults _2nd;
    public PositionStatusResults _3rd;
    public PositionStatusResults _4th;

}

[Serializable]
public class GameInfoResults
{
	public NetworkObjectData[] Items;
    public HoleStatusResults Hole;
}





