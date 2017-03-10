using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkServer;
using NetworkServer.Controllers;
using NetworkServer.Areas.Message.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace NetworkServer.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private IGameObjectDataRepository _repository = null;


        [TestMethod]
        public void GetAvailableRoomForNewLoginText()
        {
            ExecuteTest(() => {
                var player1 = new PlayerLoginModel()
                {
                    AccountName = "Testing1",
                    isRace = true,
                    UID = Guid.NewGuid().ToString()
                };

                var room1 = GetAvailableRoomForNewLogin(player1);
                var room2 = GetAvailableRoomForNewLogin(player1);


                var player2 = new PlayerLoginModel()
                {
                    AccountName = "Testing1",
                    isRace = true,
                    UID = Guid.NewGuid().ToString()
                };

                Assert.AreNotEqual(room1.sessionId, room2.sessionId);

                var room3 = GetAvailableRoomForNewLogin(player1);
                var room4 = GetAvailableRoomForNewLogin(player2);

                Assert.AreEqual(room3.sessionId, room4.sessionId);

            });
        }

        [TestMethod]
        public void GetAvailableRoomForNewLoginText2()
        {
            ExecuteTest(() => {

                List<PlayerLoginModel> players = new List<PlayerLoginModel>();
                int i = 0;
                var maxCount = 1000;
                while (i < maxCount)
                {
                    i++;
                    var id = Guid.NewGuid().ToString();
                    GetAvailableRoomForNewLogin(new PlayerLoginModel()
                    {
                        AccountName = id,
                        isRace = true,
                        UID = id
                    });
                }

                var allRooms = _repository.GetActiveRooms();
                Assert.AreEqual(maxCount / 4, allRooms.Count);

            });
        }

        [TestMethod]
        public void GetAvailableRoomForNewLoginText3()
        {
            ExecuteTest(() => {
                var player1 = new PlayerLoginModel()
                {
                    AccountName = "Testing1",
                    isRace = true,
                    UID = Guid.NewGuid().ToString()
                };

                var room1 = GetAvailableRoomForNewLogin(player1);
                var room2 = GetAvailableRoomForNewLogin(player1);


                var player2 = new PlayerLoginModel()
                {
                    AccountName = "Testing1",
                    isRace = true,
                    UID = Guid.NewGuid().ToString()
                };

                Assert.AreNotEqual(room1.sessionId, room2.sessionId);

                var room3 = GetAvailableRoomForNewLogin(player1);
                var room4 = GetAvailableRoomForNewLogin(player2);

                Assert.AreEqual(room3.sessionId, room4.sessionId);

                var allRooms1 = _repository.GetActiveRooms();
                Debug.WriteLine(string.Join(",", allRooms1.Select(x => x.status.ToString())));

                Thread.Sleep(12000);

                var allRooms2 = _repository.GetActiveRooms();
                Debug.WriteLine(string.Join(",", allRooms2.Select(x => x.status.ToString())));

                Thread.Sleep(12000);

                var allRooms3 = _repository.GetActiveRooms();
                Debug.WriteLine(string.Join(",", allRooms3.Select(x => x.status.ToString())));



            });
        }

        [TestMethod]
        public void FormatOutputData()
        {
            ExecuteTest(() => {
                string data = string.Empty;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(@"C:\Users\kyle.pearn\Source\Repos\MCG\data.txt"))
                {
                    data = sr.ReadToEnd();
                }

                var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NetworkObjectData>>(data);
                var fromTime = items.Where(x => x.accName == "Guest1701090101")
                    .OrderBy(x => x.timeStamp).Select(x => x.timeStamp).FirstOrDefault();

                var convertedItems = items.Where(x => x.accName == "Guest1701090101")
                    .OrderBy(x => x.timeStamp)
                    .Select(x => new
                    {
                        holeId = x.holeId,
                        isRace = x.isRace,
                        cannon_position_x = x.cannon_position_x,
                        cannon_position_y = x.cannon_position_y,
                        cannon_position_z = x.cannon_position_z,
                        cannon_rotation_x = x.cannon_rotation_x,
                        cannon_rotation_y = x.cannon_rotation_y,
                        cannon_rotation_z = x.cannon_rotation_z,
                        currentBullet = x.currentBullet,
                        fire = x.fire,
                        fire_accurracy = x.fire_accurracy,
                        fire_power = x.fire_power,
                        fire_torque = x.fire_torque,
                        fire_turn = x.fire_turn,
                        holeComplete = x.holeComplete,
                        root_position_x = x.root_position_x,
                        root_position_y = x.root_position_y,
                        root_position_z = x.root_position_z,
                        root_rotation_x = x.root_rotation_x,
                        root_rotation_y = x.root_rotation_y,
                        root_rotation_z = x.root_rotation_z,
                        stroke = x.stroke,
                        timeElasped = x.timeStamp.Subtract(fromTime).TotalMilliseconds,
                        waitMilliseconds = x.fire ? 1000 : x.waitMilliseconds
                    }).ToList();

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\Users\kyle.pearn\Source\Repos\MCG\data_converted.txt", false))
                {
                    sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(convertedItems));
                }

            });
        }

        private void ExecuteTest(Action action)
        {
            try
            {
                _repository = new GameObjectDataRepository();
                action();
            }
            finally
            {
                _repository.ShutDown = true;
            }
        }


        private Room GetAvailableRoomForNewLogin(PlayerLoginModel player)
        {
            return _repository.GetAvailableRoomForNewLogin(player);
            
        }



    }
}
