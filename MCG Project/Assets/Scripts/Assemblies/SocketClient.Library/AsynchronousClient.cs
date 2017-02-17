using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Linq;
using SocketClient.Library.Requests;
using SocketClient.Library.Responses;
using System.Collections.Generic;

namespace SocketClient.Library
{
    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousClient
    {
        public AsynchronousClient()
        {
            
        }

        // The port number for the remote device.
        private int _port = 11000;

        //private Socket client = null;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private String response = String.Empty;

        private Action<string> _onResponseComplete = (response) => { };
        private Action<Exception> _onError = null;
        public bool HasError { get; private set; }
        public bool Running { get; private set; }

        private void RaiseError(Exception ex)
        {
            this.HasError = true;
            if (_onError == null)
                return;
            _onError(ex);
        }

        private readonly object _lock = new object();
        private void Lock(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }

        public void ConnectSend<TRequest>(string host, int port, Action<string> onResponseComplete, Action<Exception> onError, SocketRequest<TRequest> request)
        {
            _onResponseComplete = onResponseComplete;
            _onError = onError;
            _port = port;

            // Connect to a remote device.            
            // Establish the remote endpoint for the socket.
            // The name of the 
            // remote device is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList.LastOrDefault();
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

            // Create a TCP/IP socket.
            Socket client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();
            if (this.HasError)
                return;

            Send(client, request.ToString());
            sendDone.WaitOne();

            Receive(client);
                                                
        }

        public void Connect(string host, int port, Action<string> onResponseComplete, Action<Exception> onError, string clientId)
        {
            _onResponseComplete = onResponseComplete;
            _onError = onError;
            _port = port;

            // Connect to a remote device.            
            // Establish the remote endpoint for the socket.
            // The name of the 
            // remote device is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList.LastOrDefault();
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

            // Create a TCP/IP socket.
            Socket client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();
            if (this.HasError)
                return;

            Send(client, (new SocketRequest<string> { 
                Method = "Connect",
                Service = "Server",
                ClientId = clientId,
                Data = clientId
            }).ToString());
            sendDone.WaitOne();

            
            if (_onResponseComplete == null)
                throw new Exception("Unable to complete response.");

            Receive(client);

        }


        private void ConnectCallback(IAsyncResult ar)
        {
            Socket client = null;
            try
            {
                // Retrieve the socket from the state object.
                client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);
                                
                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                RaiseError(e);
                connectDone.Set();
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                RaiseError(e);
            }
        }

        public bool CheckRequestData(string data)
        {
            return (data != null && data.IndexOf("<SDATA>") == 0 && data.IndexOf("</SDATA>") == data.Length - "</SDATA>".Length);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    response = state.sb.ToString();
                    var idx = response.IndexOf("</SDATA>");
                    if (idx > -1)
                    {
                        response = response.Substring(0, idx + "</SDATA>".Length);
                        state.sb.Remove(0, idx + "</SDATA>".Length);
                        _onResponseComplete(response);
                    }

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                //else
                //{
                //    // All the data has arrived; put it in response.
                //    if (state.sb.Length > 1)
                //    {
                //        response = state.sb.ToString();
                //        _onResponseComplete(response);
                //    }
                //    // Signal that all bytes have been received.
                //    receiveDone.Set();
                //}
            }
            catch (Exception e)
            {
                RaiseError(e);
                receiveDone.Set();
            }
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                RaiseError(e);
                sendDone.Set();
            }
        }


        

        

    }
}
