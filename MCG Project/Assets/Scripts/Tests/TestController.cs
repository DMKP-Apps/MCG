using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class TestController : MonoBehaviour {

    public string outputDirectory = "";

    //private CannonPlayerState _playerState = null;

    //public void SetPlayer(CannonPlayerState playerState)
    //{
    //    _playerState = playerState;
    //}

    private int currentHole = 1;
    private int currentCourse = 1;

    private Dictionary<int, List<NetworkObjectData>> _data = new Dictionary<int, List<NetworkObjectData>>();

    public void SetCurrentHole(int hole)
    {
        if (_data.Count > 0 && !string.IsNullOrEmpty(outputDirectory))
        {   // process and save the data...
            _data.ToList().ForEach(x => {
                var fileName = string.Format("{0}.{1}.{2}.{3}.json", currentCourse, currentHole, x.Key, Guid.NewGuid());
                var items = x.Value;
                var fromTime = items.OrderBy(i => i.timeStamp).Select(i => i.timeStamp).FirstOrDefault();

                var output = string.Format("[ {0} ]", string.Join(",", items.Select(i => new List<KeyValuePair<string, object>>() {
                    new KeyValuePair<string, object>("holeId", currentHole),
                    new KeyValuePair<string, object>("isRace", true),
                    new KeyValuePair<string, object>("cannon_position_x", i.cannon_position_x),
                    new KeyValuePair<string, object>("cannon_position_y", i.cannon_position_y),
                    new KeyValuePair<string, object>("cannon_position_z", i.cannon_position_z),
                    new KeyValuePair<string, object>("cannon_rotation_x", i.cannon_rotation_x),
                    new KeyValuePair<string, object>("cannon_rotation_y", i.cannon_rotation_y),
                    new KeyValuePair<string, object>("cannon_rotation_z", i.cannon_rotation_z),
                    new KeyValuePair<string, object>("currentBullet", i.currentBullet),
                    new KeyValuePair<string, object>("fire", i.fire),
                    new KeyValuePair<string, object>("fire_accurracy", i.fire_accurracy),
                    new KeyValuePair<string, object>("fire_power", i.fire_power),
                    new KeyValuePair<string, object>("fire_torque", i.fire_torque),
                    new KeyValuePair<string, object>("fire_turn", i.fire_turn),
                    new KeyValuePair<string, object>("holeComplete", i.holeComplete),
                    new KeyValuePair<string, object>("root_position_x", i.root_position_x),
                    new KeyValuePair<string, object>("root_position_y", i.root_position_y),
                    new KeyValuePair<string, object>("root_position_z", i.root_position_z),
                    new KeyValuePair<string, object>("root_rotation_x", i.root_rotation_x),
                    new KeyValuePair<string, object>("root_rotation_y", i.root_rotation_y),
                    new KeyValuePair<string, object>("root_rotation_z", i.root_rotation_z),
                    new KeyValuePair<string, object>("stroke", i.stroke),
                    new KeyValuePair<string, object>("timeElasped", i.timeStamp.Subtract(fromTime).TotalMilliseconds),
                    new KeyValuePair<string, object>("waitMilliseconds", i.fire ? 1000 : i.waitMilliseconds),

                })
                .Select(i => string.Format("{{ {0} }}", string.Join(", ", i.Select(j => string.Format("\"{0}\": {1}", j.Key, j.Value.ToString().ToLower())).ToArray())))
                .ToArray()));

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(string.Format("{0}\\{1}", outputDirectory.TrimEnd('\\'), fileName)))
                {
                    sw.Write(output);
                }
            });

            
        }
        currentHole = hole;
        _data = new Dictionary<int, List<NetworkObjectData>>();
    }

    public void logPlayerData(CannonPlayerState playerState, NetworkObjectData data)
    {
        if (!_data.ContainsKey(playerState.playerNumber))
        {
            _data.Add(playerState.playerNumber, new List<NetworkObjectData>());
        }
        
        _data[playerState.playerNumber].Add(data);

        Debug.Log(_data.Count);
    }

}
