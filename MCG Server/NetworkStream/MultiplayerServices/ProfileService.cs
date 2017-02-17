using MultiplayerServices.Data;
using MultiplayerServices.Data.Entity;
using MultiplayerServices.Managers;
using MultiplayerServices.Models;
using SocketServer.Library.Attributes;
using SocketServer.Library.Requests;
using SocketServer.Library.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices
{
   
    [SocketRequestService]
    public class ProfileService
    {

        [SocketRequestMethod]
        public SocketResponse<Profile> GetProfile(object data)
        {
            var request = SocketRequest.ConvertRequest<string>(data);
            var response = SocketResponse.GetResponse<Profile, string>(request);
            Profile p = null;
            MultiplayerDataContext.Execute((dc) => {
                var prop = dc.ClientPropertyBags.Where(x => x.AccountId == request.ClientId && x.Key == "profile").OrderByDescending(x => x.Id).FirstOrDefault();
                if (prop != null)
                    p = prop.Get<Profile>(new Profile() { });
                else
                {
                    p = new Profile() { };
                    prop = new Data.Entity.ClientPropertyBag() { 
                        AccountId = request.ClientId,
                        Key = "profile",
                    };
                    prop.Set(p);
                    dc.ClientPropertyBags.Add(prop);
                    dc.SaveChanges();
                }
            });
            
            response.Data = p;
            ClientManager.Clients.UpdateLastClientRequest(request.ClientId);
            return response;
        }

        [SocketRequestMethod]
        public SocketResponse<Profile> ClaimTokens(object data)
        {
            var request = SocketRequest.ConvertRequest<string>(data);
            var response = SocketResponse.GetResponse<Profile, string>(request);
            Profile p = null;
            MultiplayerDataContext.Execute((dc) =>
            {
                var prop = dc.ClientPropertyBags.Where(x => x.AccountId == request.ClientId && x.Key == "profile").OrderByDescending(x => x.Id).FirstOrDefault();
                if (prop != null)
                    p = prop.Get<Profile>(new Profile() { });
                else
                {
                    p = new Profile() { };
                    prop = new Data.Entity.ClientPropertyBag()
                    {
                        AccountId = request.ClientId,
                        Key = "profile",
                    };
                    prop.Set(p);
                    dc.ClientPropertyBags.Add(prop);
                    dc.SaveChanges();
                }


                p.ClaimTokens();
                prop.Set(p);
                dc.SaveChanges();

            });



            response.Data = p;
            ClientManager.Clients.UpdateLastClientRequest(request.ClientId);
            return response;
        }

        private List<Prize> _prizes_Characters = null;

        [SocketRequestMethod]
        public SocketResponse<Profile> ClaimPrize(object data)
        {
            var request = SocketRequest.ConvertRequest<string>(data);
            var response = SocketResponse.GetResponse<Profile, string>(request);
            Profile p = null;
            MultiplayerDataContext.Execute((dc) =>
            {
                if (_prizes_Characters == null)
                    _prizes_Characters = dc.Prizes.Where(x => x.Type == PrizeType.Character).ToList();

                var prop = dc.ClientPropertyBags.Where(x => x.AccountId == request.ClientId && x.Key == "profile").OrderByDescending(x => x.Id).FirstOrDefault();
                if (prop != null)
                    p = prop.Get<Profile>(new Profile() { });
                else
                {
                    p = new Profile() { };
                    prop = new Data.Entity.ClientPropertyBag()
                    {
                        AccountId = request.ClientId,
                        Key = "profile",
                    };
                    prop.Set(p);
                    dc.ClientPropertyBags.Add(prop);
                    dc.SaveChanges();
                }

                if (p.Tokens < p.PrizeValue) throw new Exception("Unable to claim prize. Invalid command");
                if (p.Inventory == null)
                    p.Inventory = new List<Inventory>();
                var prize = _prizes_Characters.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                p.Inventory.Add(new Inventory(prize));
                p.Tokens -= p.PrizeValue;
                prop.Set(p);
                dc.SaveChanges();

            });



            response.Data = p;
            ClientManager.Clients.UpdateLastClientRequest(request.ClientId);
            return response;
        }

    }
}
