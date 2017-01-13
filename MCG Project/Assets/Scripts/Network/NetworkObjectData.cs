using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class NetworkPlayerData
{
	public NetworkPlayerData() {
		timeStamp = DateTime.Now.ToUniversalTime();
	}

	public DateTime timeStamp;
	public string objectId;
	public string accName;
	public string sessionId;
	public string objectName;
	public bool Ready;


}


[Serializable]
public class NetworkObjectData
{
    public NetworkObjectData() {
        timeStamp = DateTime.Now.ToUniversalTime();
        fire = false;
    }

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

    public bool Compare(NetworkObjectData other)
    {
        if (other == null) {
            return false;
        }

        return this.holeId == other.holeId &&

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
            this.currentBullet == other.currentBullet;
    }

}





