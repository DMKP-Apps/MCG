
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.Web.Script.Serialization;
using SocketServer.Library.Responses;
using SocketServer.Library.Requests;
using System.Collections.Generic;
using SocketServer.Library.Attributes;
using System.IO;

namespace SocketServer.Library
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }


    public class ControllerInstance
    {
        private object _instance = null;
        private Dictionary<string, System.Reflection.MethodInfo> _methods = new Dictionary<string, System.Reflection.MethodInfo>();
        private List<System.Reflection.MethodInfo> _clientConnectMethods = new List<System.Reflection.MethodInfo>();
        private List<System.Reflection.MethodInfo> _clientDisconnectMethods = new List<System.Reflection.MethodInfo>();
        public ControllerInstance(Type type)
        {
            _instance = Activator.CreateInstance(type);
            _methods = type.GetMethods().Where(x => x.GetCustomAttributes(true).OfType<SocketRequestMethod>().Count() > 0)
                .Select(x => new { Key = x.Name.ToLower().Trim(), Value = x })
                .GroupBy(x => x.Key)
                .Select(x => new { Key = x.Key, Value = x.Where(y => y.Value.GetParameters().Count(p => p.ParameterType.Name.ToLower().Contains("object")) == 1).Select(y => y.Value).FirstOrDefault() })
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);


            _clientConnectMethods = type.GetMethods().Where(x => x.GetCustomAttributes(true).OfType<ClientConnectMethod>().Count() > 0)
                .Select(x => new { Key = x.Name.ToLower().Trim(), Value = x })
                .GroupBy(x => x.Key)
                .Select(x => new { Key = x.Key, Value = x.Where(y => y.Value.GetParameters().Count(p => p.ParameterType.Name.ToLower().Contains("string")) == 1).Select(y => y.Value).FirstOrDefault() })
                .Where(x => x.Value != null)
                .Select(x => x.Value)
                .ToList();

            _clientDisconnectMethods = type.GetMethods().Where(x => x.GetCustomAttributes(true).OfType<ClientDisconnectMethod>().Count() > 0)
                .Select(x => new { Key = x.Name.ToLower().Trim(), Value = x })
                .GroupBy(x => x.Key)
                .Select(x => new { Key = x.Key, Value = x.Where(y => y.Value.GetParameters().Count(p => p.ParameterType.Name.ToLower().Contains("string")) == 1).Select(y => y.Value).FirstOrDefault() })
                .Where(x => x.Value != null)
                .Select(x => x.Value)
                .ToList();

        }

        public bool HasValue { get { return _instance != null; } }
        public bool HasActions { get { return _methods != null && _methods.Count > 0; } }

        public bool ContainsKey(string action) { return action != null && _methods != null && _methods.ContainsKey(action.ToLower().Trim()); }

        private System.Reflection.MethodInfo GetMethod(string action) { return this.ContainsKey(action) ? _methods[action.Trim().ToLower()] : null; }

        public object Execute(SocketRequest<object> request)
        {
            var minfo = this.GetMethod(request.Method);
            if (minfo == null) throw new Exception("Method not found.");
            return minfo.Invoke(_instance, new object[] {  request });
        }

        public void Connect(string clientId)
        {
            _clientConnectMethods.ForEach(x => {
                try { x.Invoke(_instance, new object[] { clientId }); }
                catch { }
            });
        }

        public void Disconnect(string clientId)
        {
            _clientDisconnectMethods.ForEach(x => {
                try { x.Invoke(_instance, new object[] { clientId }); }
                catch { }
            });
        }


    }

    public static class SocketListener
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static Action<object> _onStatusUpdate = (data) => { };
        private static Action<Exception> _onError = (data) => { };
        private static int _port = 11000;

        private static Action<string> _onClientConnect = (clientId) => { };
        private static Action<string> _onClientDisconnect = (clientId) => { };


        private static string ControllerDirectory 
        {
            get {
                var controllerDirectory = new DirectoryInfo(string.Format("{0}\\Services", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).TrimEnd('\\')));
                if (!controllerDirectory.Exists)
                    controllerDirectory.Create();
                return controllerDirectory.FullName;
            }
        }

        private static void OnClientConnect(string clientId)
        {
            _onClientConnect(clientId);
            _controllers.ToList().ForEach(x => x.Value.Connect(clientId));
        }

        private static void OnClientDisconnect(string clientId)
        {
            _onClientDisconnect(clientId);
            _controllers.ToList().ForEach(x => x.Value.Disconnect(clientId));
        }

        private static Dictionary<string, ControllerInstance> _controllers = null;
        private static void RegisterControllers(Type[] additionalTypes)
        {
            if (_controllers != null)
                return;

            Func<string, string> getControllerName = (name) => {
                string controllerName = name.Trim().ToLower();
                string find = "service";
                if (controllerName.EndsWith(find) && controllerName.Length > find.Length)
                    controllerName = controllerName.Substring(0, controllerName.Length - find.Length);
                return controllerName;
            };

            var thisLibrary = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _controllers = new DirectoryInfo(ControllerDirectory).GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.Name != thisLibrary)
                .Select(x => System.Reflection.Assembly.LoadFrom(x.FullName))
                .SelectMany(x => x.GetTypes())
                .Union(System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                .Union(additionalTypes == null ? new Type[] { } : additionalTypes)
                .Where(x => x.GetCustomAttributes(true).OfType<SocketRequestService>().Count() > 0)
                .Select(x => new { AttributeName = x.GetCustomAttributes(true).OfType<SocketRequestService>().Select(a => a.Name).FirstOrDefault(), Name = x.Name, Value = new ControllerInstance(x) })
                .Where(x => x.Name != null && x.Value.HasValue && x.Value.HasActions)
                .Select(x => new { Key = getControllerName(string.IsNullOrEmpty(x.AttributeName) ? x.Name : x.AttributeName), Value = x.Value })
                .ToDictionary(x => x.Key, x => x.Value);

            _controllers.ToList().ForEach(x => _onStatusUpdate(string.Format("- Assembley {0}: Loaded", x.Key)));

        }

        
        public static void StartListening()
        {
            _port = System.Configuration.ConfigurationManager.AppSettings["Port"] == null ? _port : int.Parse(System.Configuration.ConfigurationManager.AppSettings["Port"]);
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList.LastOrDefault(x => x.ToString().Count(c => c == '.') == 3);
            if(!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SocketServer.IP"])) {
                ipAddress = IPAddress.Parse(System.Configuration.ConfigurationManager.AppSettings["SocketServer.IP"]);
            }
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _port);

            //_onStatusUpdate("Listening on " + ipAddress.ToString() + ":" + _port.ToString());
            _onStatusUpdate("Preparing to run socket server: " + localEndPoint.ToString());

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                //listener.RemoteEndPoint 
                listener.Listen(100);
                //listener.
                //_onStatusUpdate("I am connected to " + IPAddress.Parse(((IPEndPoint)listener.RemoteEndPoint).Address.ToString()) + "on port number " + ((IPEndPoint)listener.RemoteEndPoint).Port.ToString());

                //// Using the LocalEndPoint property.
                //_onStatusUpdate("My local IpAddress is :" + IPAddress.Parse(((IPEndPoint)listener.LocalEndPoint).Address.ToString()) + "I am connected on port number " + ((IPEndPoint)listener.LocalEndPoint).Port.ToString());

                while (!_hasExit)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    _onStatusUpdate("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                _onError(e);
            }

        }

        private static bool _hasExit = false;

        public static void Shutdown()
        {
            _hasExit = true;
            allDone.Set();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            SocketRequest<object> request = null;
            try
            {

                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read 
                    // more data.
                    content = state.sb.ToString();
                    if (SocketRequest.CheckRequestData(content))
                    {
                        request = SocketRequest.GetRequest<object>(content);
                        
                        // All the data has been read from the 
                        // client. Display it on the console.
                        _onStatusUpdate(string.Format("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content));
                        // Echo the data back to the client.
                        List<SocketRequest<object>> loopRequest = new List<SocketRequest<object>>();
                        //requestData.ForEach(request => {
                        if (request == null)
                            throw new Exception("Invalid request data.");
                        if (string.IsNullOrEmpty(request.Service))
                            throw new Exception("Invalid request data. No controller specified.");
                        if (string.IsNullOrEmpty(request.Method))
                            throw new Exception("Invalid request data. No controller action specified.");
                        if (!_controllers.ContainsKey(request.Service.ToLower().Trim()))
                            throw new Exception(string.Format("Unable to located controller '{0}'", request.Service));

                        var controller = _controllers[request.Service.ToLower().Trim()];
                        if (!controller.ContainsKey(request.Method))
                            throw new Exception(string.Format("Unable to located controller action '{0}.{1}'", request.Service, request.Method));

                        var result = controller.Execute(request);
                            

                        if (request.Service == "Server" && request.Method == "Connect")
                        {
                            Send(handler, result);
                            Thread messageThread = new Thread(new ParameterizedThreadStart((args) => {
                                var resp = controller.Execute(args as SocketRequest<object>);
                                int cnt = 0;
                                OnClientConnect((args as SocketRequest<object>).ClientId);
                                while (resp != null)
                                {
                                    cnt++;
                                    if (((SocketResponse<object>)resp).Data != null || cnt >= 60)
                                    {
                                        cnt = 0;
                                        if (((SocketResponse<object>)resp).Data == null)
                                            ((SocketResponse<object>)resp).Data = new List<object>();
                                        // Convert the string data to byte data using ASCII encoding.
                                        byte[] byteData = Encoding.ASCII.GetBytes(resp.ToString());
                                        // Begin sending the data to the remote device.
                                        try
                                        {
                                            handler.BeginSend(byteData, 0, byteData.Length, 0,
                                            new AsyncCallback(SendCallback), handler);
                                        }
                                        catch 
                                        {
                                            resp = null;
                                            _onStatusUpdate(string.Format("Client disconnected - {0}", (args as SocketRequest<object>).ClientId));
                                            OnClientDisconnect((args as SocketRequest<object>).ClientId);
                                            break;
                                        }
                                    }
                                    
                                    Thread.Sleep(1000);
                                    resp = controller.Execute(args as SocketRequest<object>);
                                    if (_hasExit)
                                    {
                                        resp = null;
                                        break;
                                    }
                                }                                
                            }));
                            messageThread.Start(new SocketRequest<object>() { 
                                ClientId = request.ClientId,
                                Service = "Server",
                                Method = "Message"
                            });
                        }
                        else
                            Send2(handler, result);

                        
                        
                    }
                    else
                    {
                        // Not all data received. Get more.
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception ex)
            {
                var responseError = new SocketResponse<object>() {
                    ClientId = request != null ? request.ClientId : string.Empty,
                    RequestId = request != null ? request.RequestId : string.Empty,
                    DataType = ex.GetType().ToString(),
                    Message = ex.Message,
                    Status = false,
                    Data = null
                };
                Send2(handler, responseError);
            }
        }

        private static void Send(Socket handler, object data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data.ToString());
            // Begin sending the data to the remote device.
            try
            {
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                _onError(e);
            }
        }
        private static void Send2(Socket handler, object data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data.ToString());
            // Begin sending the data to the remote device.
            try
            {
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback2), handler);
            }
            catch (Exception e)
            {
                _onError(e);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                _onStatusUpdate(string.Format("Sent {0} bytes to client.", bytesSent));

            }
            catch (Exception e)
            {
                _onError(e);
            }
        }

        private static void SendCallback2(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                _onStatusUpdate(string.Format("Sent {0} bytes to client.", bytesSent));

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                _onError(e);
            }
        }

        public static void Run(Action<object> onStatusUpdate, Action<Exception> onError)
        {
            Run(onStatusUpdate, null, null, onError, null, 0);
        }
        public static void Run(Action<object> onStatusUpdate, Action<Exception> onError, Type[] additionalTypes)
        {
            Run(onStatusUpdate, null, null, onError, additionalTypes, 0);
        }
        public static void Run(Action<object> onStatusUpdate, Action<Exception> onError, int port)
        {
            Run(onStatusUpdate, null, null, onError, null, port);
        }
        public static void Run(Action<object> onStatusUpdate, Action<Exception> onError, Type[] additionalTypes, int port)
        {
            Run(onStatusUpdate, null, null, onError, additionalTypes, port);
        }
        public static void Run(Action<object> onStatusUpdate, Action<string> onClientConnect, Action<string> onClientDisconnect, Action<Exception> onError, Type[] additionalTypes, int port)
        {
            _onStatusUpdate = onStatusUpdate != null ? onStatusUpdate : (data) => { };
            _onClientConnect = onClientConnect != null ? onClientConnect : (data) => { };
            _onClientDisconnect = onClientDisconnect != null ? onClientDisconnect : (data) => { };
            _onError = onError != null ? onError : (data) => { };
            if (port > 0)
                _port = port;
            if (additionalTypes == null && System.Reflection.Assembly.GetCallingAssembly() != null)
                additionalTypes = System.Reflection.Assembly.GetCallingAssembly().GetTypes();

            

            RegisterControllers(additionalTypes);
            StartListening();
        }
    }
}
