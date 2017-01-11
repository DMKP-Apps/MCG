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
        public IMessageRepository Repository { get; set; }

        public RequestController()
        {
            this.Repository = DependencyResolver.Current.GetService<IMessageRepository>();
        }

        [HttpGet]
        public async Task<ActionResult> Ping()
        {
            var messages = await Task.Run<IEnumerable<MessageItem>>(() => Repository.GetAll());
            var data = await Task.Run<string>(() => string.Format("Successfully ping at {0} - {1} Requests in queue", System.DateTime.Now, messages.Count()));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var data = await Task.Run<IEnumerable<MessageItem>>(() => Repository.GetAll());
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetById(string id)
        {
            var data = await Task.Run<MessageItem>(() => Repository.Find(id));
            return Json(data, JsonRequestBehavior.AllowGet);
            
        }

        [HttpPost]
        public async Task<ActionResult> Create(MessageItem model)
        {
            var data = await Task.Run<MessageItem>(() => Repository.Add(model));
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<ActionResult> SaveObjectData(NetworkObjectData model)
        {
            //var data = await Task.Run<MessageItem>(() => Repository.Add(model));
            var data = await Task.Run<string>(() => {
                SaveNetworkObjectData(model);
                return "Success";
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        

        [HttpPost]
        public async Task<ActionResult> SaveActionData(NetworkActionShotData model)
        {
            //var data = await Task.Run<MessageItem>(() => Repository.Add(model));
            var data = await Task.Run<string>(() => {
                SaveNetworkObjectData(model);
                return "Success";
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        private void SaveNetworkObjectData(NetworkObjectData model)
        {
            using (MCGDbContext context = new MCGDbContext())
            {
                Guid sessionId = Guid.Empty;
                if (!Guid.TryParse(model.sessionId, out sessionId)) {
                    sessionId = Guid.Empty;
                }
                GameObjectData entity = new GameObjectData()
                {
                    objectId = model.objectId,
                    objectName = model.objectName,
                    SessionId = sessionId,
                    holeId = model.holeId,
                    type = model.GetType().Name,
                    data = Newtonsoft.Json.JsonConvert.SerializeObject(model)
                };
                context.GameObjectData.Add(entity);
                context.SaveChanges();
            }
        }


        [HttpPost]
        public async Task<ActionResult> Login(Player model)
        {
            var data = await Task.Run<Player>(() => { 
                using (MCGDbContext context = new MCGDbContext())
                {
                    var player = context.Players.FirstOrDefault(x => x.UID == model.UID);
                    if (player != null)
                    {
                        model = player;
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

                        context.Players.Add(model);
                        context.SaveChanges();
                    }
                }

                return model;
            });
            //var data = await Task.Run<MessageItem>(() => Repository.Add(model));
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        /*[HttpGet("{id}", Name = "GetMessage")]
        public IActionResult GetById(string id)
        {
            var item = Repository.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }*/

    }
}
