using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetworkServer.Areas.Message.Models
{
    public class PlayerLoginModel
    {
        public string UID { get; set; }
        public string AccountName { get; set; }
        public bool isRace { get; set; }
    }
    public abstract class NetworkData
    {
        public NetworkData()
        {
            timeStamp = DateTime.Now.ToUniversalTime();
            uniqueId = Guid.NewGuid().ToString();
            type = this.GetType().Name;
        }
        public string type { get; set; }
        public string uniqueId { get; set; }
        public string Key { get { return objectId; } }
        public DateTime timeStamp { get; set; }
        public string objectId { get; set; }
        public string accName { get; set; }
        public string sessionId { get; set; }
        public bool isRace { get; set; }

    }

    public class NetworkWaitingData : NetworkData
    {
        public NetworkWaitingData() : base()
        {
            Ready = false;
        }

        public bool Ready { get; set; }
    }

    public class NetworkObjectData : NetworkWaitingData
    {
        public NetworkObjectData() : base()
        {
            fire = false;
            playerType = "default";
            waitMilliseconds = 0;
        }
        
        public string playerType { get; set; }
        public string objectName { get; set; }
        public string holeId { get; set; }

        public float root_position_x { get; set; }
        public float root_position_y { get; set; }
        public float root_position_z { get; set; }

        public float root_rotation_x { get; set; }
        public float root_rotation_y { get; set; }
        public float root_rotation_z { get; set; }

        public float cannon_position_x { get; set; }
        public float cannon_position_y { get; set; }
        public float cannon_position_z { get; set; }

        public float cannon_rotation_x { get; set; }
        public float cannon_rotation_y { get; set; }
        public float cannon_rotation_z { get; set; }

        public int stroke { get; set; }
        public bool fire { get; set; }
        public float fire_torque { get; set; }
        public float fire_turn { get; set; }
        public float fire_power { get; set; }
        public float fire_accurracy { get; set; }

        public double waitMilliseconds { get; set; }

        public int currentBullet { get; set; }
        public bool holeComplete { get; set; }

        public void UpdateWaitMillieseconds() {
            if (waitMilliseconds <= 0) {
                return; // do nothing
            }
            var currentDateTime = DateTime.Now.ToUniversalTime();
            var milliseconds = currentDateTime.Subtract(timeStamp).TotalMilliseconds;

            waitMilliseconds = waitMilliseconds - milliseconds;
            if (waitMilliseconds < 0) {
                waitMilliseconds = 0;
            }

        }

    }

   
    
}