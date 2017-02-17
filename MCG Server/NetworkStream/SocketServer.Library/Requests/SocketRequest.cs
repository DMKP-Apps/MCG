using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SocketServer.Library.Requests
{
    public class SocketRequest
    {
        public static bool CheckRequestData(string data)
        {
            return (data != null && data.IndexOf("<SDATA>") == 0 && data.IndexOf("</SDATA>") == data.Length - "</SDATA>".Length);
        }
        public static SocketRequest<T> GetRequest<T>(string data)
        {
            if (data == null || data.IndexOf("<SDATA>") != 0 || data.IndexOf("</SDATA>") != data.Length - "</SDATA>".Length)
                return new SocketRequest<T>() { Data = default(T) };

            data = data.Substring("<SDATA>".Length);
            data = data.Substring(0, data.Length - "</SDATA>".Length).Trim();

            if (string.IsNullOrEmpty(data))
                return new SocketRequest<T>() { Data = default(T) };
            var result = data.FromJsonString<SocketRequest<object>>();
            return SocketRequest.ConvertRequest<T>(result);

        }

        //public static List<SocketRequest<T>> GetRequests<T>(string data)
        //{
        //    if (data == null || data.IndexOf("<SDATA>") != 0 || data.IndexOf("</SDATA>") != data.Length - "</SDATA>".Length)
        //        return new List<SocketRequest<T>>();

        //    data = data.Substring("<SDATA>".Length);
        //    data = data.Substring(0, data.Length - "</SDATA>".Length).Trim();

        //    if (string.IsNullOrEmpty(data))
        //        return new List<SocketRequest<T>>();
        //    var requestData = data.FromJsonString<List<SocketRequest<object>>>();
        //    return requestData.Select(result => SocketRequest.ConvertRequest<T>(result)).ToList();
            

        //}

        public static SocketRequest<T> ConvertRequest<T>(object data)
        {
            if (!(data is SocketRequest<object>))
                return null;
            if (typeof(T) == typeof(object))
                return ((SocketRequest<T>)data);

            T cdata = default(T);
            try { cdata = ((SocketRequest<object>)data).Data == null ? default(T) : ((SocketRequest<object>)data).Data.ToJsonString().FromJsonString<T>(); }
            catch { }
            return new SocketRequest<T>()
            {
                Method = ((SocketRequest<object>)data).Method,
                Service = ((SocketRequest<object>)data).Service,
                ClientId = ((SocketRequest<object>)data).ClientId,
                RequestId = ((SocketRequest<object>)data).RequestId,
                Data = cdata,
            };
        }

    }


    public abstract class SocketRequestBase
    {
        public string ClientId { get; set; }
        public string RequestId { get; set; }
        public string Method { get; set; }
        public string Service { get; set; }
    }

    public class SocketRequest<TData> : SocketRequestBase
    {
        public TData Data { get; set; }
        public override string ToString()
        {
            return string.Format("<SDATA>{0}</SDATA>", this.ToJsonString());
        }

    }
}
