using NetworkServer.Areas.Message.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using System.Threading.Tasks;
using NetworkServer.Library.Entities;
using NetworkServer.Library;

namespace NetworkServer.Areas.Message.Controllers
{
    
    public class RequestController : Controller
    {
        public IGameObjectDataRepository Repository { get; set; }

        public RequestController()
        {
            this.Repository = DependencyResolver.Current.GetService<IGameObjectDataRepository>();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Rooms()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Ping()
        {
            var messages = await Task.Run<IEnumerable<NetworkData>>(() => Repository.GetAll());
            var data = await Task.Run<string>(() => string.Format("Successfully ping at {0} - {1} Requests in queue", System.DateTime.Now, messages.Count()));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var data = await Task.Run<IEnumerable<NetworkData>>(() => Repository.GetAll());
            return Json(new { Items = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetRooms()
        {
            var data = await Task.Run<IEnumerable<object>>(() => Repository.GetActiveRooms().Select(x => new {
                sessionId = x.sessionId,
                maxAttendance = x.maxAttendance,
                minAttendance = x.minAttendance,
                created = x.created.ToString(),
                lastRequestTime = x.lastRequestTime.ToString(),
                type = x.type,
                course = x.course,
                currentHole = x.currentHole,
                nextPhaseOn = x.nextPhaseOn.HasValue ? x.nextPhaseOn.Value.ToString() : string.Empty,
                attendeesActive = x.attendees.Where(a => !a.Value.Removed).Select(a => a.Value),
                attendeesAll = x.attendees.Select(a => a.Value),
                status = x.status
            }));
            return Json(new { Items = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetPlayerRoom(string id)
        {
            var data = await Task.Run<object>(() => {
                return Repository.GetActiveRooms().Where(x => x.attendees.Any(a => a.Value.UID == id && !a.Value.Removed))
                    .Select(x => new {
                        sessionId = x.sessionId,
                        maxAttendance = x.maxAttendance,
                        minAttendance = x.minAttendance,
                        created = DateTime.Now.Subtract(x.created).TotalMilliseconds,
                        type = x.type,
                        course = x.course,
                        currentHole = x.currentHole,
                        nextPhaseOn = x.nextPhaseOn.HasValue ? x.nextPhaseOn.Value.Subtract(DateTime.Now).TotalMilliseconds : -1,
                        status = x.status,
                        attendees = x.attendees.Select(a => a.Value),
                    }).FirstOrDefault();
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public async Task<ActionResult> GetRoomStatus(string id)
        {
            var data = await Task.Run<object>(() => {
                var room = Repository.GetRoomByKey(id);
                if (!string.IsNullOrWhiteSpace(id) && room == null)
                {
                    return null;
                }
                if (room == null) {
                    return null;
                }
                return new
                {
                    sessionId = room.sessionId,
                    maxAttendance = room.maxAttendance,
                    minAttendance = room.minAttendance,
                    created = DateTime.Now.Subtract(room.created).TotalMilliseconds,
                    type = room.type,
                    course = room.course,
                    currentHole = room.currentHole,
                    nextPhaseOn = room.nextPhaseOn.HasValue ? room.nextPhaseOn.Value.Subtract(DateTime.Now).TotalMilliseconds : -1,
                    status = room.status,
                    attendees = room.attendees.Select(a => new
                    {
                        UID = a.Value.UID,
                        AccountName = a.Value.AccountName,
                        playerNumber = a.Value.playerNumber,
                        position = a.Value.position,
                        Removed = a.Value.Removed
                    }).ToList(),
                };
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public async Task<ActionResult> GetById(string id)
        {
            var data = await Task.Run<object>(() => Repository.Find(id));
            return Json(data, JsonRequestBehavior.AllowGet);
            
        }

        
        [HttpPost]
        public async Task<ActionResult> SaveObjectData(NetworkObjectData model)
        {
            //var data = await Task.Run<MessageItem>(() => Repository.Add(model));
            var data = await Task.Run<string>(() => {
                Repository.Add(model);

                if (model.holeComplete) {
                    // need to get the room and update the players rank.
                    var room = Repository.GetRoomByKey(model.sessionId);
                    if (room != null) {
                        room.PlayerCompletedHole(model.objectId);
                    }
                }

                return "Success";
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        

        [HttpPost]
        public async Task<ActionResult> SaveWaitingData(NetworkWaitingData model)
        {
            //var data = await Task.Run<MessageItem>(() => Repository.Add(model));
            var data = await Task.Run<string>(() => {
                Repository.Add(model);
                return "Success";
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public async Task<ActionResult> GetNetworkPlayers(string id)
        {
            var data = await Task.Run<IEnumerable<NetworkWaitingData>>(() =>
            {
                var info = Repository.Find(id);
                if (info != null) {
                    Repository.Update(info);
                    return Repository.GetAll<NetworkWaitingData>()
                        .Where(x => x.sessionId == info.sessionId);
                }

                return new List<NetworkWaitingData>();
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> NetworkPlayerReady(string id)
        {
            var data = await Task.Run<string>(() =>
            {
                var info = Repository.Find(id) as NetworkWaitingData;
                if (info != null)
                {
                    info.Ready = true;
                    Repository.Update(info);
                    return info.sessionId;
                }

                return string.Empty;
            });
            return Json(data, JsonRequestBehavior.AllowGet);


        }

        
        [HttpGet]
        public async Task<ActionResult> IsPlayerOnline(string id)
        {
            var data = await Task.Run<NetworkData>(() =>
            {
                return Repository.Find(id);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetGameInfo(string id)
        {
            var data = await Task.Run<NetworkObjectData>(() =>
            {
                return Repository.Find(id) as NetworkObjectData;
            });

            List<NetworkObjectData> result = null;
            Room room = null;
            string sessionId = string.Empty;
            if (data != null) {
                sessionId = data.sessionId;
                // locate all other players...
                result = Repository.GetAll<NetworkObjectData>()
                    .Where(x => x.sessionId == sessionId).ToList();
                result.ForEach(x => x.UpdateWaitMillieseconds());

                room = Repository.GetRoomByKey(sessionId);
            }


            var obj = result != null && room != null ? new
            {
                Items = result,
                Hole = new {
                    status = room.status,
                    _1st = room.attendees.Where(x => !x.Value.Removed && x.Value.rankings.ContainsKey(room.currentHole) && x.Value.rankings[room.currentHole] == 1).Select(x => new {
                        UID = x.Value.UID,
                        playerNumber = x.Value.playerNumber
                    }).FirstOrDefault(),
                    _2nd = room.attendees.Where(x => !x.Value.Removed && x.Value.rankings.ContainsKey(room.currentHole) && x.Value.rankings[room.currentHole] == 2).Select(x => new {
                        UID = x.Value.UID,
                        playerNumber = x.Value.playerNumber
                    }).FirstOrDefault(),
                    _3rd = room.attendees.Where(x => !x.Value.Removed && x.Value.rankings.ContainsKey(room.currentHole) && x.Value.rankings[room.currentHole] == 3).Select(x => new {
                        UID = x.Value.UID,
                        playerNumber = x.Value.playerNumber
                    }).FirstOrDefault(),
                    _4th = room.attendees.Where(x => !x.Value.Removed && x.Value.rankings.ContainsKey(room.currentHole) && x.Value.rankings[room.currentHole] == 4).Select(x => new {
                        UID = x.Value.UID,
                        playerNumber = x.Value.playerNumber
                    }).FirstOrDefault()

                },
            } : null;
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetGameInfoBySessionId(string id)
        {
            var data = await Task.Run<List<NetworkObjectData>>(() =>
            {
                List<NetworkObjectData> result = null;
                
                    // locate all other players...
                    result = Repository.GetAll<NetworkObjectData>()
                        .Where(x => x.sessionId == id).ToList();
                    result.ForEach(x => x.UpdateWaitMillieseconds());

                
                return result;
            });

            


            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public async Task<ActionResult> Login(PlayerLoginModel model)
        {
            var data = await Task.Run<PlayerLoginModel>(() => {
                return ExecuteLogin(model);

            });
            //var data = await Task.Run<MessageItem>(() => Repository.Add(model));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public async Task<ActionResult> LoginWithSession(PlayerLoginModelWithSession model)
        //{
        //    var data = await Task.Run<PlayerLoginModel>(() => {

        //        return ExecuteLogin(model);

        //    });
        //    //var data = await Task.Run<MessageItem>(() => Repository.Add(model));
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        private PlayerLoginModel ExecuteLogin(PlayerLoginModel model)
        {
            if (model.AccountName != GameObjectDataRepository.auto_usernameKey)
            {
                using (MCGDbContext context = new MCGDbContext())
                {
                    var player = context.Players.FirstOrDefault(x => x.UID == model.UID);
                    if (player != null)
                    {
                        model.AccountName = player.AccountName;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(model.AccountName))
                        {
                            string accountName = string.Format("Guest{0}{1}{2}", DateTime.Now.Year.ToString().Substring(2), DateTime.Now.Month.ToString("00"), DateTime.Now.Day.ToString("00"));
                            int currentInterval = 1;
                            string checkName = string.Format("{0}{1}", accountName, currentInterval.ToString("00"));
                            while (context.Players.Any(x => x.AccountName == checkName))
                            {
                                currentInterval++;
                                checkName = string.Format("{0}{1}", accountName, currentInterval.ToString("00"));
                            }
                            model.AccountName = string.Format("{0}{1}", accountName, currentInterval.ToString("00"));
                        }
                        else if (context.Players.Any(x => x.AccountName == model.AccountName))
                        {
                            int currentInterval = 1;
                            string checkName = string.Format("{0}{1}", model.AccountName, currentInterval.ToString("00"));
                            while (context.Players.Any(x => x.AccountName == checkName))
                            {
                                currentInterval++;
                                checkName = string.Format("{0}{1}", model.AccountName, currentInterval.ToString("00"));
                            }
                            model.AccountName = string.Format("{0}{1}", model.AccountName, currentInterval.ToString("00"));
                        }

                        context.Players.Add(new Player() { UID = model.UID, AccountName = model.AccountName });
                        context.SaveChanges();
                    }
                }
            }
            else
            {
                string accountName = string.Format("Guest{0}{1}{2}{3}", DateTime.Now.Year.ToString().Substring(2), DateTime.Now.Month.ToString("00"), DateTime.Now.Day.ToString("00"), DateTime.Now.Millisecond.ToString("00").Substring(0, 2));
                model.AccountName = accountName;
            }


            var room = Repository.GetAvailableRoomForNewLogin(model);
            if (room == null)
            {
                throw new Exception("No rooms available.");
            }
            var sessionId = room.sessionId;

            NetworkWaitingData clientInfo = new NetworkWaitingData()
            {
                accName = model.AccountName,
                objectId = model.UID,
                sessionId = sessionId,
                isRace = model.isRace,
                Ready = true
            };

            model.isRace = room.type == GameType.Race;

            Repository.Add(clientInfo);

            return model;
        }


        [HttpGet]
        public async Task<ActionResult> Logout(string id)
        {
            var data = await Task.Run<bool>(() =>
            {
                Repository.RemovePlayerFromRoom(id);
                Repository.Remove(id);
                return true;
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}
