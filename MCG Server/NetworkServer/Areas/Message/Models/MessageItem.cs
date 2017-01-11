using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NetworkServer.Areas.Message.Models
{
    public class MessageItem
    {
        public string Key { get; set; }
        public string Request { get; set; }
        public string Data { get; set; }
        public string Response { get; set; }
        public bool IsComplete { get; set; }
    }

    public interface IMessageRepository
    {
        MessageItem Add(MessageItem item);
        IEnumerable<MessageItem> GetAll();
        MessageItem Find(string key);
        MessageItem Remove(string key);
        void Update(MessageItem item);
    }

    public class MessageRepository : IMessageRepository
    {
        private static ConcurrentDictionary<string, MessageItem> _messages =
              new ConcurrentDictionary<string, MessageItem>();

        public MessageRepository()
        {
            
        }

        public IEnumerable<MessageItem> GetAll()
        {
            return _messages.Values;
        }

        public MessageItem Add(MessageItem item)
        {
            item.Key = Guid.NewGuid().ToString();
            _messages[item.Key] = item;
            return item;
        }

        public MessageItem Find(string key)
        {
            MessageItem item;
            _messages.TryGetValue(key, out item);
            return item;
        }

        public MessageItem Remove(string key)
        {
            MessageItem item;
            _messages.TryRemove(key, out item);
            return item;
        }

        public void Update(MessageItem item)
        {
            _messages[item.Key] = item;
        }
    }
}