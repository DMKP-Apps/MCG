using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NetworkServer.Areas.Message.Models
{
    public interface INetworkDataRepository
    {
        NetworkData Add(NetworkData item);
        IEnumerable<NetworkData> GetAll();
        NetworkData Find(string key);
        NetworkData Remove(string key);

        IEnumerable<TNetworkData> GetAll<TNetworkData>() where TNetworkData : NetworkData;

        void Update(NetworkData item);
        bool ShutDown { get; set; }
    }

    public class NetworkDataRepository : INetworkDataRepository
    {
        private static ConcurrentDictionary<string, NetworkData> _messages =
              new ConcurrentDictionary<string, NetworkData>();

        public bool ShutDown { get; set; }
        private DateTime _lastCheckTime = DateTime.Now;
        public NetworkDataRepository()
        {
            ShutDown = false;
            var thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((args) => {
                while (!ShutDown)
                {
                    if (DateTime.Now.Subtract(_lastCheckTime).TotalMinutes > 5)
                    {
                        _lastCheckTime = DateTime.Now;
                        ClearStaleSessions();
                    }
                    System.Threading.Thread.Sleep(500);
                }
                
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        private void ClearStaleSessions()
        {
            var queryDate = DateTime.Now.ToUniversalTime();
            var oldMessages = _messages.Values.Where(x => queryDate.Subtract(x.timeStamp).TotalMinutes > 10).ToList();
            oldMessages.ForEach(x => Remove(x.Key));
            
        }

        public IEnumerable<TNetworkData> GetAll<TNetworkData>()
            where TNetworkData : NetworkData
        {
            var result = _messages.Values.OfType<TNetworkData>();
            return result;
        }

        public IEnumerable<NetworkData> GetAll()
        {
            return _messages.Values;
        }

        public NetworkData Add(NetworkData item)
        {
            item.timeStamp = DateTime.Now.ToUniversalTime();
            item.type = item.GetType().Name;
            if (string.IsNullOrWhiteSpace(item.uniqueId))
            {
                item.uniqueId = Guid.NewGuid().ToString();
            }
            _messages[item.Key] = item;

            return item;
        }

        public NetworkData Find(string key)
        {
            NetworkData item;
            _messages.TryGetValue(key, out item);
            return item;
        }

        public NetworkData Remove(string key)
        {
            NetworkData item;
            _messages.TryRemove(key, out item);
            return item;
        }

        public void Update(NetworkData item)
        {
            item.timeStamp = DateTime.Now.ToUniversalTime();
            item.type = item.GetType().Name;
            if (string.IsNullOrWhiteSpace(item.uniqueId))
            {
                item.uniqueId = Guid.NewGuid().ToString();
            }
            _messages[item.Key] = item;
        }
    }
}