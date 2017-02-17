using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketClient.Library.Requests
{
    public abstract class SocketRequestBase
    {
        public string ClientId { get; set; }
        public string RequestId { get; set; }
        public string Method { get; set; }
        public string Service { get; set; }

    }

    
    public class SocketRequest<TData> : SocketRequestBase
    {
        public SocketRequest()
        {
            ClientId = string.Empty;
            RequestId = Guid.NewGuid().ToString("N");
        }
        public TData Data { get; set; }
        public override string ToString()
        {
            return string.Format("<SDATA>{0}</SDATA>", this.ToJsonString());
        }

    }



}
