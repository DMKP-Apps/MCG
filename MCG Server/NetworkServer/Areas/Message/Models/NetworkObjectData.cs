using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetworkServer.Areas.Message.Models
{
    
    public class NetworkObjectData
    {
        public NetworkObjectData()
        {
            timeStamp = DateTime.Now.ToUniversalTime();
            fire = false;
        }
        public string Key { get { return objectId; } }
        public DateTime timeStamp { get; set; }
        public string objectId { get; set; }
        public string accName { get; set; }
        public string sessionId { get; set; }
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

        public bool fire { get; set; }
        public float fire_torque { get; set; }
        public float fire_turn { get; set; }
        public float fire_power { get; set; }
        public int currentBullet { get; set; }
    }

   
    
}