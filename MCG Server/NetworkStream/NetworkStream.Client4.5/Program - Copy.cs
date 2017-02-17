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

        public class ClientInfo
        {
            public string ID { get; set; }
            public string Name { get; set; }
        }

        public class Room
        {
            public Room()
            {
                this.ID = Guid.NewGuid().ToString("N");
                this.Clients = new List<ClientInfo>();
            }
            public string ID { get; set; }

            public List<ClientInfo> Clients { get; set; }
        }

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

        private static string _currentPlayerId = string.Empty;

        static List<int> _spinValues = new List<int>() { 0, 0, 0, 0, 0 };

        static void ResetSpin()
        {
            _spinValues = new List<int>() { 0, 0, 0, 0, 0 };
        }
        static void Spin()
        {
            Thread.Sleep(1000);
            Spin(0);
            Spin(1);
            Spin(2);
            Spin(3);
            Spin(4);
        }
        static void Spin(int index)
        {
            var values = new List<int>() { 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6, };

            if (_spinValues[index] > 0) return;
            Thread go = new Thread(new ParameterizedThreadStart((idx) =>
            {
                Random r = new Random();
                var wait = r.Next(1, 10);
                var value = values.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                int loop = 0;
                while (loop < wait && _currentPlayerId == client.ID)
                {
                    loop++;
                    value = values.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                    Thread.Sleep(100);
                }

                _spinValues[(int)idx] = value;

            }));
            go.Start(index);
            
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

                ResetSpin();
                bool stay = false;
                int turn = 0;
                while (!stay)
                {
                    turn++;
                    Spin();
                    bool spinning = true;

                    while (spinning)
                    {
                        Lock(() => spinning = _spinValues.Any(x => x < 1));
                        Thread.Sleep(150);
                    }

                    Console.WriteLine("Spin {0}: {1},{2},{3},{4},{5}", turn, _spinValues[0], _spinValues[1], _spinValues[2], _spinValues[3], _spinValues[4]);
                    if (_currentPlayerId != client.ID)
                        return;
                    var spinresults = _spinValues.GroupBy(x => x)
                        .Select(x => new { Count = x.Count(), Value = x.FirstOrDefault()})
                        .ToList();
                    stay = spinresults.Count == 5 || (spinresults.Count == 2 && spinresults.Any(x => x.Count == 3)) || spinresults.Count == 1;
                    stay = turn > 2 ? true : stay;
                    client.SendRequest<object>(new { RoomId = data, Value1 = _spinValues[0], Value2 = _spinValues[1], Value3 = _spinValues[2], Value4 = _spinValues[3], Value5 = _spinValues[4],
                        Turn = turn,
                        Stay = stay
                    }, "Spin", "Game");

                    if (stay)
                        break;

                    if (spinresults.Count == 4)
                    {
                        var resetValue = spinresults.Where(x => x.Count > 1).Select(x => x.Value).FirstOrDefault();
                        _spinValues[_spinValues.FindLastIndex(x => x == resetValue)] = 0;
                    }
                    else if (spinresults.Count == 3)
                    {
                        var resetValue = spinresults.Where(x => x.Count == 1).Select(x => x.Value).FirstOrDefault();
                        _spinValues[_spinValues.FindLastIndex(x => x == resetValue)] = 0;
                    }
                    else if (spinresults.Any(x => x.Count == 3))
                    {
                        var resetValue = spinresults.Where(x => x.Count == 3).Select(x => x.Value).FirstOrDefault();
                        while (_spinValues.FindIndex(x => x != resetValue && x != 0) > -1)
                        {
                            _spinValues[_spinValues.FindIndex(x => x != resetValue && x != 0)] = 0;
                        }
                    }
                    else if (spinresults.Any(x => x.Count == 4))
                    {
                        var resetValue = spinresults.Where(x => x.Count == 4).Select(x => x.Value).FirstOrDefault();
                        while (_spinValues.FindIndex(x => x != resetValue && x != 0) > -1)
                        {
                            _spinValues[_spinValues.FindIndex(x => x != resetValue && x != 0)] = 0;
                        }
                    }


                }
                               
                
            });

            Dictionary<string, Action<object>> actions = new Dictionary<string, Action<object>>();
            actions.Add("Server.Message".ToLower(), (data) =>
            {
                var obj = data.ToJsonString().FromJsonString<List<Message>>();
                obj.Where(x => x.Type == "ChatAlertMessage").ToList().ForEach(x => Console.WriteLine("* {0}", x.Value));

                obj.Where(x => x.Type == "CurrentPlayerMessage").ToList().ForEach(x => {
                    var request = x.Value as Dictionary<string, object>;
                    _currentPlayerId = request["PlayerId"] as string;
                    var index = (int)request["Index"];
                    
                    
                });


                obj.Where(x => x.Type == "RequestActionMessage").ToList().ForEach(x =>
                {
                    var request = x.Value as Dictionary<string, object>;
                    var currentAction = string.Format("{0}.{1}", request["Service"], request["Method"]);
                    if (handleRequestActions.ContainsKey(currentAction))
                    {
                        Thread reqAction = new Thread(new ParameterizedThreadStart((arg) => {
                            handleRequestActions[currentAction](request["Data"]);
                        }));
                        reqAction.Start();
                        
                    }
                    
                });
            });
            actions.Add("Game.Logon".ToLower(), (data) =>
            {
                var obj = data.ToJsonString().FromJsonString<string>();
                client.Name = obj;
                client.SendRequest<string>(client.Name, "GetProfile", "Profile");
                //client.SendRequest<string>(client.Name, "Join", "Game");
            });

            Action outputInventory = () => {
                var obj = get<Dictionary<string, object>>("profile");
                var inventory = obj["Inventory"].ToJsonString().FromJsonString<List<Dictionary<string, object>>>();
                Console.WriteLine("\r\nInventory:");
                // output the inventory
                var items = inventory.Select(x => new { Prize = x["Prize"].ToJsonString().FromJsonString<Dictionary<string, object>>(), Level = (int)x["Level"], Id = new Guid(x["Id"].ToString()) })
                    .Select(x => new
                    {
                        Level = x.Level,
                        Id = x.Id,
                        PrizeId = (int)x.Prize["Id"],
                        Name = x.Prize["Name"].ToString(),
                        ShortName = x.Prize["ShortName"].ToString(),
                        Type = (int)x.Prize["Type"] == 1 ? "Character" : "Perk"
                    }).ToList();
                
                items.ForEach(x => Console.WriteLine("\t* LVL {0} - {1} ({2})", x.Level.ToString("00"), x.ShortName, x.Name));

                bool inGame = get<bool>("ingame");
                if (!inGame) { 
                    // get a random character,
                    var plyItem = items.OrderBy(x => Guid.NewGuid()).Take(1).Select(x => new
                    {
                        Id = x.Id,
                        Level = x.Level,
                        Prize = new { 
                            Id = x.PrizeId,
                            Name = x.Name,
                            ShortName = x.ShortName,
                            Type = x.Type == "Character" ? 1 : 2
                        }
                    }).FirstOrDefault();

                    client.SendRequest<object>(plyItem, "Join", "Game");
                }

            };

            actions.Add("Profile.GetProfile".ToLower(), (data) =>
            {
                var obj = data.ToJsonString().FromJsonString<Dictionary<string,object>>();
                set("profile", obj);
                var nextPrizeOn = ((DateTime)obj["NextPrizeOn"]).ToLocalTime();
                Console.WriteLine("Tokens: {0}", obj["Tokens"]);
                Console.WriteLine("Next prize on: {0}", nextPrizeOn);
                Console.WriteLine("Collect: {0}", DateTime.Now > nextPrizeOn);
                if (DateTime.Now > nextPrizeOn)
                    client.SendRequest<string>(string.Empty, "ClaimTokens", "Profile");
                else if ((int)obj["PrizeValue"] <= (int)obj["Tokens"])
                    client.SendRequest<string>(string.Empty, "ClaimPrize", "Profile");
                else
                    outputInventory();
            });

            actions.Add("Profile.ClaimTokens".ToLower(), (data) =>
            {
                var obj = data.ToJsonString().FromJsonString<Dictionary<string, object>>();
                set("profile", obj);
                Console.WriteLine("Tokens Update: {0}", obj["Tokens"]);
                if ((int)obj["PrizeValue"] <= (int)obj["Tokens"])
                {
                    client.SendRequest<string>(string.Empty, "ClaimPrize", "Profile");
                    return;
                }

                outputInventory();

            });

            actions.Add("Profile.ClaimPrize".ToLower(), (data) =>
            {
                var obj = data.ToJsonString().FromJsonString<Dictionary<string, object>>();
                set("profile", obj);
                var inventory = obj["Inventory"].ToJsonString().FromJsonString<List<Dictionary<string, object>>>();
                var newitem = inventory.LastOrDefault()["Prize"].ToJsonString().FromJsonString<Dictionary<string, object>>();
                Console.WriteLine("You just won: {0}", newitem["Name"]);
                Console.WriteLine("Number of Items: {0}", inventory.Count);
                Console.WriteLine("Tokens Update: {0}", obj["Tokens"]);

                outputInventory();

            });

            actions.Add("Game.Join".ToLower(), (data) =>
            {


                var obj = data.ToJsonString().FromJsonString<Room>();
                set("Game", obj);
                Console.WriteLine("Connected to room: {0}", obj.ID);
                client.SendRequest<string>(obj.ID, "GetClients", "Game");
            });
            actions.Add("Game.GetClients".ToLower(), (data) =>
            {
                var obj = data.ToJsonString().FromJsonString<List<ClientInfo>>();
                Console.WriteLine("Peers in room: {0}", obj.Count);
                obj.ForEach(x => Console.WriteLine("* {0} [ID: {1}]", x.Name, x.ID));
            });

            if (actions.ContainsKey(actionKey))
                actions[actionKey](response);
            
            //client.SendRequest<List<ClientInfo>, string>(room.ID, "GetClients", "Room", (data) =>
            //{
            //    if (!data.Status) WriteError(data.Message);
            //    else
            //    {
            //        room.Clients = data.Data;
            //        Console.WriteLine("Peers in room: {0}", room.Clients.Count);
            //        room.Clients.ForEach(x => Console.WriteLine("* {0} [ID: {1}]", x.Name, x.ID));
            //    }
            //});
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

            //while (!connectRoomResponse.Status)
            //{
            //    WriteError(connectRoomResponse.Message);
            //    var key = Confirm("Attempt to retry the request?");
            //    if (key == 'n')
            //    {
            //        Console.WriteLine("\nGood bye!");
            //        return;
            //    }
            //    Console.WriteLine("\nAttempting to join remote room...");
            //    connectRoomResponse = client.SendRequest<Room, string>(currentAction.GetDataAs<string>(), currentAction.Action, currentAction.Controller);
            //}

            //Room room = connectRoomResponse.Data;
            //Console.WriteLine("Connected to room: {0}", room.ID);
            //currentAction = new RequestAction(room.ID, "GetClients", "Room");
            //var getClientsResponse = client.SendRequest<List<ClientInfo>, string>(currentAction.GetDataAs<string>(), currentAction.Action, currentAction.Controller);
            //if (!getClientsResponse.Status)
            //{
            //    WriteError(connectRoomResponse.Message);
            //    AnyKey("Unable to continue!");
            //    Console.WriteLine("\nGood bye!");
            //    return;
            //}

            //List<ClientInfo> clients = getClientsResponse.Data;
            //Console.WriteLine("Peers in room: {0}", clients.Count);
            //clients.ForEach(x => Console.WriteLine("* {0} [ID: {1}]", x.Name, x.ID));
            //Thread checkRoom = new Thread(new ParameterizedThreadStart((data) => {
            //    while (!Exit)
            //    {
            //        Thread.Sleep(2000);
            //        getClientsResponse = client.SendRequest<List<ClientInfo>, string>(currentAction.GetDataAs<string>(), currentAction.Action, currentAction.Controller);
            //        if (!getClientsResponse.Status)
            //        {
            //            WriteError(connectRoomResponse.Message);
            //        }
            //        var addclients = getClientsResponse.Data.Where(x => !clients.Any(y => y.ID == x.ID)).ToList();
            //        var removeClients = clients.Where(x => !getClientsResponse.Data.Any(y => y.ID == x.ID)).ToList();
            //        clients = getClientsResponse.Data;
            //        removeClients.ForEach(x => Console.WriteLine("- {0} has left the room!", x.Name, x.ID));
            //        addclients.ForEach(x => Console.WriteLine("+ {0} has joined the room!", x.Name, x.ID));
            //    }

            //}));
            //checkRoom.Start();

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
