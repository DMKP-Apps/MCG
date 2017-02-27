using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCGServer.Client.Models
{

    [Serializable]
    public class NetworkPlayerData
{
        public NetworkPlayerData()
        {
            timeStamp = DateTime.Now.ToUniversalTime();
            fire = false;
            playerType = "default";
            uniqueId = Guid.NewGuid().ToString();
            Ready = false;
            isRace = false;
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


    }

    [Serializable]
    public class GameInfoResults
    {
        public NetworkPlayerData[] Items;
    }
}
