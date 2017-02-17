using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SocketServer.Library.Attributes
{
    public class SocketRequestAttributes : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class SocketRequestMethod : SocketRequestAttributes
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ClientConnectMethod : SocketRequestAttributes
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ClientDisconnectMethod : SocketRequestAttributes
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SocketRequestService : SocketRequestAttributes
    {
        public SocketRequestService()
        { 
            
        }

        public SocketRequestService(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }

    
}
