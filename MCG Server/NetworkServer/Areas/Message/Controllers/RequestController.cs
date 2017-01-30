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
        public INetworkDataRepository Repository { get; set; }

        public RequestController()
        {
            this.Repository = DependencyResolver.Current.GetService<INetworkDataRepository>();
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
                    attendees = room.attendees.Select(a => a.Value),
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

                _items.Add(model);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\Users\kyle.pearn\Source\Repos\MCG\data.txt", false))
                {
                    sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(_items));
                }

                return "Success";
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        private List<NetworkObjectData> _items = new List<NetworkObjectData>();

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
            if (data != null) {
                // locate all other players...
                result = Repository.GetAll<NetworkObjectData>()
                    .Where(x => x.sessionId == data.sessionId).ToList();
                result.ForEach(x => x.UpdateWaitMillieseconds());
                    
            }


            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public async Task<ActionResult> Login(PlayerLoginModel model)
        {
            var data = await Task.Run<PlayerLoginModel>(() => { 
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
                        else if(context.Players.Any(x => x.AccountName == model.AccountName))
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

                var room = Repository.GetAvailableRoomForNewLogin(model);
                if (room == null)
                {
                    throw new Exception("No rooms available.");
                }
                var sessionId = room.sessionId;
                
                NetworkWaitingData clientInfo = new NetworkWaitingData() {
                    accName = model.AccountName,
                    objectId = model.UID,
                    sessionId = sessionId,
                    isRace = model.isRace
                };

                Repository.Add(clientInfo);

                return model;


            });
            //var data = await Task.Run<MessageItem>(() => Repository.Add(model));
            return Json(data, JsonRequestBehavior.AllowGet);
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
