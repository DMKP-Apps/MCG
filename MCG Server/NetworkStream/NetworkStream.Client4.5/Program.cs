using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketClient.Library;

namespace NetworkStream
{
    class Program
    {

        public class Message
        {
            public Message()
            {
            }

            public object Value { get; set; }
            public string Type { get; set; }
            public object Source { get; set; }
        }


        public class RequestAction
        {
            public RequestAction(object data, string action, string controller)
            {
                this.Controller = controller;
                this.Action = action;
                this.Data = data;
            }
            public string Controller { get; set; }
            public string Action { get; set; }
            public object Data { get; set; }

            public T GetDataAs<T>()
            {
                if (Data == null)
                    return default(T);
                return (T)Data;
            }
        }

        static void WriteError(string message)
        {
            Console.WriteLine("\nERROR!\n{0}\n", message);
        }

        static char Confirm(string message)
        {
            Console.Write("\n{0} [y|n] ", message);
            var key = Console.ReadKey();
            while (key.KeyChar != 'n' && key.KeyChar != 'y')
            {
                Console.Write("\n! Invalid key, press 'y' for yes, 'n' for no: ");
                key = Console.ReadKey();
            }
            return key.KeyChar;
        }

        static char AnyKey(string message)
        {
            Console.WriteLine("\n{0} [press any key to continue] ", message);
            return Console.ReadKey().KeyChar;
        }

        

        public static bool Exit { get; set; }

        private static Dictionary<string, object> _propertyBag = new Dictionary<string, object>();
        static T get<T>(string key)
        {
            if (!_propertyBag.ContainsKey(key))
                return default(T);
            return (T)_propertyBag[key];
        }
        static void set<T>(string key, T value)
        {
            if (!_propertyBag.ContainsKey(key))
                _propertyBag.Add(key, value);
            else
                _propertyBag[key] = value;
        }




        private static readonly object _lock = new object();
        private static void Lock(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }
        static void OnServerMessage<T>(SocketClient.Library.Responses.SocketResponse<T> responseObj)
        {
            var response = responseObj.Data;
            var actionKey = string.Format("{0}.{1}", responseObj.Service, responseObj.Method).ToLower();

            Dictionary<string, Action<object>> handleRequestActions = new Dictionary<string, Action<object>>();
            handleRequestActions.Add("Game.Spin", (data) => {

                
            });

            //Dictionary<string, Action<object>> actions = new Dictionary<string, Action<object>>();

            //actions.Add("Game.Logon".ToLower(), (data) =>
            //{
            //    var obj = data.ToJsonString().FromJsonString<string>();
            //    client.Name = obj;
            //    client.SendRequest<string>(client.Name, "GetProfile", "Profile");
            //    client.SendRequest<string>(client.Name, "Join", "Game");
            //});


            //if (actions.ContainsKey(actionKey))
            //    actions[actionKey](response);

            
        }

        static Client client = null;
        static void Main(string[] args)
        {

            client = new Client("a89d0a4eb26642f899fb7d63aac69147");//Guid.NewGuid().ToString("N"));
            client.Name = RandomClient();
            // on load attempt to connect to service
            Console.WriteLine("Attempting to join remote room...");

            var status = client.StartClient(System.Configuration.ConfigurationManager.AppSettings["Host"], 
                int.Parse(System.Configuration.ConfigurationManager.AppSettings["Port"]),
                (data) =>
            {
                if (!data.Status) WriteError(data.Message);
                else OnServerMessage(data); 
            });

            client.SendRequest<string>(client.ID, "Logon", "Game");


            while (!Exit) 
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("\nGood bye!");
            Thread.Sleep(300);
        }

        private static string RandomReadLine()
        {
            string data = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus lacus magna, porttitor eu felis in, dignissim dictum enim. Donec sagittis nulla a erat elementum accumsan. Aliquam ipsum lectus, fermentum ac viverra eu, bibendum at nisi. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed feugiat metus euismod malesuada blandit. Cras et lacus ut nunc tempor ultricies sit amet a diam. Integer in lectus blandit sem porta ultrices a sed nunc. Sed sed ultricies tortor. Pellentesque leo turpis, condimentum eget lacinia et, molestie sodales lacus. Nullam fringilla lacinia enim, id finibus quam maximus vel. Sed at tincidunt justo. Cras purus velit, consequat at neque a, pharetra tempor nibh. Aenean porta porttitor pharetra. Sed eu pellentesque ligula, non feugiat felis. Donec et tortor vitae risus volutpat gravida. Sed varius turpis in elit pellentesque porta.";
            var input = data.Split('.').Select(x => x.Trim()).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            Thread.Sleep(1000);
            return input;
        }

        private static string RandomClient()
        {
            string data = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus lacus magna, porttitor eu felis in, dignissim dictum enim. Donec sagittis nulla a erat elementum accumsan. Aliquam ipsum lectus, fermentum ac viverra eu, bibendum at nisi. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed feugiat metus euismod malesuada blandit. Cras et lacus ut nunc tempor ultricies sit amet a diam. Integer in lectus blandit sem porta ultrices a sed nunc. Sed sed ultricies tortor. Pellentesque leo turpis, condimentum eget lacinia et, molestie sodales lacus. Nullam fringilla lacinia enim, id finibus quam maximus vel. Sed at tincidunt justo. Cras purus velit, consequat at neque a, pharetra tempor nibh. Aenean porta porttitor pharetra. Sed eu pellentesque ligula, non feugiat felis. Donec et tortor vitae risus volutpat gravida. Sed varius turpis in elit pellentesque porta.";
            var input = data.Split(" ,.;:'-".ToCharArray()).Select(x => x.Trim()).Where(x => x.Length > 4).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            return input;
        }
    }
}
