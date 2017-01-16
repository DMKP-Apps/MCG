using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NetworkServer.Areas.Message.Models
{
    public interface IGameObjectDataRepository
    {
        NetworkObjectData Add(NetworkObjectData item);
        IEnumerable<NetworkObjectData> GetAll();
        NetworkObjectData Find(string key);
        NetworkObjectData Remove(string key);
        void Update(NetworkObjectData item);
    }

    public class GameObjectDataRepository : IGameObjectDataRepository
    {
        private static ConcurrentDictionary<string, NetworkObjectData> _messages =
              new ConcurrentDictionary<string, NetworkObjectData>();

        public GameObjectDataRepository()
        {
            
        }

        public IEnumerable<NetworkObjectData> GetAll()
        {
            return _messages.Values;
        }

        public NetworkObjectData Add(NetworkObjectData item)
        {
            _messages[item.Key] = item;
            return item;
        }

        public NetworkObjectData Find(string key)
        {
            NetworkObjectData item;
            _messages.TryGetValue(key, out item);
            return item;
        }

        public NetworkObjectData Remove(string key)
        {
            NetworkObjectData item;
            _messages.TryRemove(key, out item);
            return item;
        }

        public void Update(NetworkObjectData item)
        {
            _messages[item.Key] = item;
        }
    }
}