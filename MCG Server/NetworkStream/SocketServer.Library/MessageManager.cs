using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SocketServer.Library
{
    public class Message
    {
        public Message(object value)
        {
            Value = value;
            Type = this.GetType().Name;
        }

        public object Value { get; set; }
        public string Type { get; set; }
        public object Source { get; set; }
    }

    

    public static class MessageManager
    {
        private static Dictionary<string, List<Message>> _messages = new Dictionary<string, List<Message>>();
        private static readonly object _lock = new object();
        private static void Lock(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }
        public static void Add(string clientId, Message value)
        {
            Lock(() => { 
                if(!_messages.ContainsKey(clientId))
                    _messages.Add(clientId, new List<Message>() { value });
                else
                    _messages[clientId].Add(value);
            });
        }

        public static List<Message> GetMessages(string clientId)
        {
            if (!_messages.ContainsKey(clientId))
                return new List<Message>();

            List<Message> messages = new List<Message>();
            Lock(() =>
            {
                messages = _messages[clientId];
                _messages.Remove(clientId);
            });

            return messages;
        }

    }
}
