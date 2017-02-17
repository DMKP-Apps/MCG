using SocketClient.Library.Requests;
using SocketClient.Library.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SocketClient.Library
{
    public class Client
    {
        public Client()
        {
            ID = Guid.NewGuid().ToString("N");
        }
        public Client(string id)
        {
            ID = id;
        }
        private bool CheckRequestData(string data)
        {
            return (data != null && data.IndexOf("<SDATA>") == 0 && data.IndexOf("</SDATA>") == data.Length - "</SDATA>".Length);
        }

        private SocketResponse<T> GetResponse<T>(string data)
        {
            if (data == null || data.IndexOf("<SDATA>") != 0 || data.IndexOf("</SDATA>") != data.Length - "</SDATA>".Length)
                return new SocketResponse<T>() { Data = default(T) };

            data = data.Substring("<SDATA>".Length);
            data = data.Substring(0, data.Length - "</SDATA>".Length).Trim();

            if (string.IsNullOrEmpty(data))
                return new SocketResponse<T>() { Data = default(T) };
            var result = data.FromJsonString<SocketResponse<object>>();
            return ConvertResponse<T>(result);

        }

        
        public SocketResponse<T> ConvertResponse<T>(object data)
        {
            if (!(data is SocketResponse<object>))
                return null;
            if (typeof(T) == typeof(object))
                return ((SocketResponse<T>)data);

            T cdata = default(T);
            try { cdata = ((SocketResponse<object>)data).Data == null ? default(T) : ((SocketResponse<object>)data).Data.ToJsonString().FromJsonString<T>(); }
            catch { }
            return new SocketResponse<T>()
            {
                ClientId = ((SocketResponse<object>)data).ClientId,
                Data = cdata,
                DataType = typeof(T).ToString(),
                Message = ((SocketResponse<object>)data).Message,
                RequestId = ((SocketResponse<object>)data).RequestId,
                Status = ((SocketResponse<object>)data).Status,
                Service = ((SocketResponse<object>)data).Service,
                Method = ((SocketResponse<object>)data).Method,
            };
        }


        public string ID { get; set; }
        public string Name { get; set; }

        public void SendRequest<TRequest>(TRequest data, string action, string controller)
        {
            SendRequest<TRequest>(new SocketRequest<TRequest>()
            {
                ClientId = this.ID,
                RequestId = Guid.NewGuid().ToString("N"),
                Data = data,
                Method = action,
                Service = controller               
            });
        }
       
        AsynchronousClient client = new AsynchronousClient();

        public void SendRequest<TRequest>(SocketRequest<TRequest> request)
        {
            try
            {
                AsynchronousClient sendclient = new AsynchronousClient();
                try
                {
                    Action<string> onResponse = (data) =>
                    {
                        _onRequestComplete(GetResponse<object>(data));
                    };
                    Action<Exception> onError = (error) =>
                    {
                        _onRequestComplete(new SocketResponse<object>()
                        {
                            Status = false,
                            Message = error.Message,
                            DataType = typeof(object).Name,
                        });
                    };

                    sendclient.ConnectSend(_host, _port, onResponse, onError, request);
                }
                catch (Exception error)
                {
                    _onRequestComplete(new SocketResponse<object>()
                    {
                        Status = false,
                        Message = error.Message,
                        DataType = typeof(object).Name,
                    });
                }

            }
            catch (Exception error)
            {
                _onRequestComplete(new SocketResponse<object>()
                {
                    Status = false,
                    Message = error.Message,
                    DataType = typeof(object).Name,
                    Service = request.Service,
                    Method = request.Method,
                    ClientId = request.ClientId,
                    RequestId = request.RequestId
                });
            }

        }



        Action<SocketResponse<object>> _onRequestComplete = (data) => { };
        private string _host = string.Empty;
        private int _port = 11000;
        public bool StartClient(string host, int port, Action<SocketResponse<object>> onRequestComplete)
        {
            _onRequestComplete = onRequestComplete;
            _host = host;
            _port = port;
            bool? status = null;
            try
            {
                Action<string> onResponse = (data) =>
                {
                    var response = GetResponse<object>(data);
                    if (response.Service == "Server" && response.Method == "Connect")
                        status = (bool)response.Data;
                    else
                        _onRequestComplete(response);
                };
                Action<Exception> onError = (error) =>
                {
                    _onRequestComplete(new SocketResponse<object>()
                    {
                        Status = false,
                        Message = error.Message,
                        DataType = typeof(object).Name,
                    });
                    
                };

                client.Connect(host, port, onResponse, onError, this.ID);
            }
            catch (Exception error)
            {
                _onRequestComplete(new SocketResponse<object>()
                {
                    Status = false,
                    Message = error.Message,
                    DataType = typeof(object).Name,
                });
            }

            while (!status.HasValue)
            {
                Thread.Sleep(100);
            }

            return status.Value;

        }


    }

}
