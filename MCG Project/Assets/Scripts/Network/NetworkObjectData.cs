using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class NetworkObjectData
{
    public NetworkObjectData() {
        timeStamp = DateTime.Now;
    }

    public DateTime timeStamp;
    public string objectId;
    public string sessionId;
    public string objectName;

    public float position_x;
    public float position_y;
    public float position_z;

    public float rotation_x;
    public float rotation_y;
    public float rotation_z;

    public string holeId;
}

public class NetworkActionShotData : NetworkObjectData
{
    public float torque;
    public float turn;
    public float power;
}



