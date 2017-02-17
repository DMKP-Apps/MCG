using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkStream.Responses
{

    public abstract class SocketResponseBase
    {
        public string ClientId { get; set; }
        public string RequestId { get; set; }
        public string DataType { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }

        public string Method { get; set; }
        public string Service { get; set; }
    }

    public class SocketResponse<TData> : SocketResponseBase
    {
        public SocketResponse()
        {
            ClientId = string.Empty;
            RequestId = Guid.NewGuid().ToString("N");
            Status = false;
            Method = string.Empty;
            Service = string.Empty;
        }
        public TData Data { get; set; }
        public override string ToString()
        {
            return string.Format("<SDATA>{0}</SDATA>", this.ToJsonString());
        }

    }
}
