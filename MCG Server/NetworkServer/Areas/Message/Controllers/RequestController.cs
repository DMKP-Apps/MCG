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

            IEnumerable<NetworkObjectData> result = null;
            if (data != null) {
                // locate all other players...
                result = Repository.GetAll<NetworkObjectData>()
                    .Where(x => x.sessionId == data.sessionId);
                    
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

                var sessionId = Guid.NewGuid().ToString();
                Repository.GetAll<NetworkWaitingData>()
                    .GroupBy(x => x.sessionId)
                    .Where(x => x.Count() < 2)
                    .Select(x => x.FirstOrDefault())
                    .Where(x => x != null && x.isRace == model.isRace)
                    .OrderBy(x => x.timeStamp)
                    .Take(1).ToList().ForEach(n => sessionId = n.sessionId);

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
                Repository.Remove(id);
                return true;
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}
