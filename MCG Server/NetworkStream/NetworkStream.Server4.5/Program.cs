using SocketServer.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkStream.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 62820);
            //using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            //{
                SocketListener.Run((data) => {
                    Console.WriteLine(data);
                }, (error) => {
                    Console.WriteLine(error.ToString());
                });
            //}

            Console.ReadLine();
        }
    }
}
