using SocketServer.Library.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SocketServer.Library.Responses
{
    public class SocketResponse
    {
        public static SocketResponse<TResponse> GetResponse<TResponse, TRequest>(SocketRequest<TRequest> request)
        {
            //TResponse data = request.Data.ToJsonString().FromJsonString<TResponse>();
            return new SocketResponse<TResponse>()
            {
                ClientId = request.ClientId,
                RequestId = request.RequestId,
                Status = true,
                Message = "Command completed successfully",
                DataType = typeof(TResponse).ToString(),
                Data = default(TResponse),
                Method = request.Method,
                Service = request.Service
            };
        }
    }


    public abstract class SocketResponseBase
    {
        public string ClientId { get; set; }
        public string RequestId { get; set; }
        public string DataType { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }

        //public List<SocketRequest<object>> LoopRequests { get; set; }

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
            //LoopRequests = new List<SocketRequest<object>>();
            Method = string.Empty;
            Service = string.Empty;
        }

        public TData Data { get; set; }
        public override string ToString()
        {
            return string.Format("<SDATA>{0}</SDATA>", this.ToJsonString());
        }
    }

    //public class SocketResponseCollection
    //{
    //    public SocketResponseCollection() { }
    //    public SocketResponseCollection(IEnumerable<object> data)
    //    {
    //        _data = data != null ? data.ToList() : new List<object>();
    //    }

    //    private List<object> _data = new List<object>();
    //    public void Add(object data)
    //    {
    //        _data.Add(data);
    //    }

    //    public void AddRange(IEnumerable<object> data)
    //    {
    //        _data.AddRange(data);
    //    }

    //    public void Remove(object data)
    //    {
    //        _data.Remove(data);
    //    }

    //    public void Clear(object data)
    //    {
    //        _data = new List<object>();
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("<SDATA>{0}</SDATA>", _data.ToJsonString());
    //    }

    //}


}
