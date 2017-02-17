using SocketServer.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Messages
{
    public class ChatAlertMessage : Message
    {
        public ChatAlertMessage()
            : base(null)
        {
        }

        public ChatAlertMessage(object value)
            : base(value)
        {
        }
    }

    public class RequestActionMessage : Message
    {
        
        public RequestActionMessage(string service, string method, object data)
            : base(new { Service = service, Method = method, Data = data })
        {
        }
    }

    public class GameOverMessage : Message
    {

        public GameOverMessage()
            : base("Game Over")
        {
        }
    }

    public class CurrentPlayerMessage : Message
    {

        public CurrentPlayerMessage(string player, int index)
            : base(new { PlayerId = player, Index = index })
        {
        }
    }
}
