using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCGServer.Client.Models
{
    public class NetworkObjectData
    {
        public NetworkObjectData()
        {
            
            fire = false;
            waitMilliseconds = 0;
        }

        public string objectId { get; set; }

        public string accName { get; set; }
        public string sessionId { get; set; }

        public double timeElasped { get; set; }

        private DateTime created = DateTime.Now;
        public DateTime timeStamp { get { return created.AddMilliseconds(timeElasped); } }

        public bool isRace { get; set; }
                
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

        

    }

}
