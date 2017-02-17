using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketClient.Library.Requests
{
    public abstract class SocketRequestBase
    {
        public string ClientId;
        public string RequestId;
        public string Method;
        public string Service;

    }

    
    public class SocketRequest<TData> : SocketRequestBase
    {
        public SocketRequest()
        {
            ClientId = string.Empty;
            RequestId = Guid.NewGuid().ToString("N");
        }
        public TData Data;
        public override string ToString()
        {
            return string.Format("<SDATA>{0}</SDATA>", this.ToJsonString());
        }

    }



}
