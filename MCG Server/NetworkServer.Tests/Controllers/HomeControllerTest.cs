using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkServer;
using NetworkServer.Controllers;
using NetworkServer.Areas.Message.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NetworkServer.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private INetworkDataRepository _repository = null;


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

                Thread.Sleep(12000);

                var allRooms2 = _repository.GetActiveRooms();

                Thread.Sleep(12000);

                var allRooms3 = _repository.GetActiveRooms();



            });
        }

        private void ExecuteTest(Action action)
        {
            try
            {
                _repository = new NetworkDataRepository();
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
