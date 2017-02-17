using MultiplayerServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketServer.Library;
using MultiplayerServices.Data;

namespace MultiplayerServices.Managers
{
    public class ClientCollection
    {
        private static readonly object _lock = new object();
        private static void Lock(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }

        public void UpdateLastClientRequest(string clientId)
        {
            Thread update = new Thread(new ParameterizedThreadStart((args) => {
                var cId = args.ToString();
                MultiplayerDataContext.Execute(dc => {
                    var entity = dc.Clients.FirstOrDefault(x => x.Id == cId);
                    if (entity != null)
                    {
                        if (entity.IsLockedOut)
                        {
                            throw new Exception("Access Denied, Client request has been blocked.");
                        }

                        entity.LastRequestOn = DateTime.Now.ToUniversalTime();
                        entity.IsOnline = true;
                        dc.SaveChanges();
                    }
                });
            }));
            update.IsBackground = true;
            update.Start(clientId);

            
        }

        

        public void SetClientOffline(string clientId)
        {
            Thread update = new Thread(new ParameterizedThreadStart((args) =>
            {
                var cId = args.ToString();
                MultiplayerDataContext.Execute(dc =>
                {
                    var entity = dc.Clients.FirstOrDefault(x => x.Id == cId);
                    if (entity != null)
                    {
                        entity.IsOnline = false;
                        dc.SaveChanges();
                    }
                });
            }));
            update.IsBackground = true;
            update.Start(clientId);
        }

        public Client this[string clientId]
        {
            get
            {
                Client c = null;
                MultiplayerDataContext.Execute(dc => {
                    var entity = dc.Clients.FirstOrDefault(x => x.Id == clientId);
                    if (entity != null)
                    {
                        c = new Client() { ID = entity.Id, Name = entity.Username };
                    }
                    else
                    {
                        c = new Client() { ID = clientId, Name = string.Format("Guest_{0}", (dc.Clients.Count() + 1).ToString("00000")) };
                        dc.Clients.Add(new Data.Entity.ClientAccount()
                        {
                            Id = c.ID,
                            Username = c.Name,
                        });
                        dc.SaveChanges();
                    }
                });
                return c;
            }
            set
            {
                MultiplayerDataContext.Execute(dc => {
                    var entity = dc.Clients.FirstOrDefault(x => x.Id == clientId);
                    if (entity != null && entity.Username != value.Name)
                    {
                        entity.Username = value.Name;
                        entity.ModifiedOn = DateTime.Now.ToUniversalTime();
                        dc.SaveChanges();
                    }
                    else
                    {
                        dc.Clients.Add(new Data.Entity.ClientAccount() {
                            Id = value.ID,
                            Username = value.Name,
                        });
                        dc.SaveChanges();
                    }
                });
            }
        }

        public bool HasKey(string clientId)
        {
            bool result = false;
            MultiplayerDataContext.Execute(dc => {
                result = dc.Clients.Any(x => x.Id == clientId);
            });
            return result;
        }
    }

    public static class ClientManager
    {
        private static ClientCollection _manager = new ClientCollection();
        public static ClientCollection Clients { get { return _manager; } }
    }
}
