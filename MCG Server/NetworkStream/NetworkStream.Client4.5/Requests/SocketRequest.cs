using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkStream.Requests
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

    //public class SocketRequestCollection
    //{
    //    public SocketRequestCollection() { }
    //    public SocketRequestCollection(IEnumerable<object> data)
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
