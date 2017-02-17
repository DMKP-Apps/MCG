using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketClient.Library.Responses
{

    public abstract class SocketResponseBase
    {
        public string ClientId;
        public string RequestId;
        public string DataType;

        public bool Status;
        public string Message;

        public string Method;
        public string Service;
    }

    public class SocketResponse : SocketResponseBase
    {
        public SocketResponse()
        {
            ClientId = string.Empty;
            RequestId = Guid.NewGuid().ToString("N");
            Status = false;
            Method = string.Empty;
            Service = string.Empty;
        }
        public object Data;
        public override string ToString()
        {
            return string.Format("<SDATA>{0}</SDATA>", this.ToJsonString());
        }

    }
}
