using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetworkServer.Areas.Message.Models
{
    //public class TestObject
    //{

    //}
    public class NetworkObjectData
    {
        public DateTime timeStamp { get; set; }
        public string objectId { get; set; }
        public string sessionId { get; set; }
        public string objectName { get; set; }
        public string holeId { get; set; }

        public float position_x { get; set; }
        public float position_y { get; set; }
        public float position_z { get; set; }

        public float rotation_x { get; set; }
        public float rotation_y { get; set; }
        public float rotation_z { get; set; }
    }

   
    public class NetworkActionShotData : NetworkObjectData
    {
        public float torque { get; set; }
        public float turn { get; set; }
        public float power { get; set; }
    }
}