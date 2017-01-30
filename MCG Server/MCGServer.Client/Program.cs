using MCGServer.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MCGServer.Client
{
    class Program
    {
        private static string _url = "http://localhost:8321/";

        private static Dictionary<string, Action> _commands = new Dictionary<string, Action>(); 


        static void Main(string[] args)
        {
            init();
            string cmd = string.Empty;
            while (cmd != null && cmd.ToLower() != "exit")
            {
                if (!string.IsNullOrWhiteSpace(cmd) && _commands.ContainsKey(cmd.ToLower()))
                {

                    // execute command
                    _commands[cmd.ToLower()]();

                }
                else if (!string.IsNullOrWhiteSpace(cmd) && !_commands.ContainsKey(cmd.ToLower()))
                {
                    Console.WriteLine(string.Format("* Command '{0}' does not exist.", cmd));
                }
                else if (string.IsNullOrWhiteSpace(cmd))
                {
                    _commands["refresh"]();
                }

                Console.Write("> ");
                cmd = Console.ReadLine();
            }

            _players.ForEach(x => Logout(x));


            //var result = Login(Guid.NewGuid().ToString(), "Guest001", true);
            //Console.WriteLine(result);
        }

        static List<playerInfo> _players = new List<playerInfo>();

        private static void init() {
            _commands.Add("login", PromptLogin);
            _commands.Add("logout", PromptLogout);
            _commands.Add("pinfo", PromptPlayerDetails);
            _commands.Add("refresh", RefreshData);
        }

        private static void PromptLogin()
        {
            Console.Write("[1] Username: ");
            var accountName = Console.ReadLine();
            Console.Write("[2] is race (Y/N): ");
            var isRace = Console.ReadKey();
            Console.WriteLine("");
            Console.WriteLine(string.Format("Logging in as user '{0}'...", accountName));
            var race = isRace.KeyChar.ToString().ToUpper() == "Y";

            var player = new playerInfo()
            {
                UID = Guid.NewGuid().ToString(),
                AccountName = accountName,
                isRace = race
            };

            player = Login(player);
            _players.Add(player);
            GetPlayerRoom(player);

            Console.WriteLine(string.Format("Players Logged in '{0}'...", _players.Count));
        }

        private static void PromptLogout()
        {
            Console.WriteLine("What player to logout? ");

            int i = 0;
            _players.ForEach(p => {
                i++;
                Console.WriteLine("[{0}] {1}", i, p.AccountName);
            });

            Console.Write(string.Format("Index [1-{0}]: ", _players.Count));
            var playerKey = Console.ReadKey().KeyChar;

            int index = 0;
            if (!int.TryParse(playerKey.ToString(), out index))
            {
                Console.WriteLine("");
                Console.WriteLine(string.Format("Invalid key value '{0}'.", playerKey));
            }
            else
            {
                if (index < 1 || index > _players.Count)
                {
                    Console.WriteLine("");
                    Console.WriteLine(string.Format("Index is out of range '{0}'.", playerKey));
                }
                else
                {
                    var result = Logout(_players[index - 1]);
                    Console.WriteLine("");
                    Console.WriteLine(string.Format("Player logout: {0}", result));

                    if (result)
                    {
                        _players.RemoveAt(index - 1);
                    }
                }
            }
            
        }

        private static void PromptPlayerDetails()
        {
            Console.WriteLine("What player to view? ");

            int i = 0;
            _players.ForEach(p => {
                i++;
                Console.WriteLine("[{0}] {1}", i, p.AccountName);
            });

            Console.Write(string.Format("Index [1-{0}]: ", _players.Count));
            var playerKey = Console.ReadKey().KeyChar;

            int index = 0;
            if (!int.TryParse(playerKey.ToString(), out index))
            {
                Console.WriteLine("");
                Console.WriteLine(string.Format("Invalid key value '{0}'.", playerKey));
            }
            else
            {
                if (index < 1 || index > _players.Count)
                {
                    Console.WriteLine("");
                    Console.WriteLine(string.Format("Index is out of range '{0}'.", playerKey));
                }
                else
                {
                    var result = _players[index - 1];
                    Console.WriteLine("");
                    Console.WriteLine(string.Format("Player information: {0}", result.AccountName));
                    if (result is playerInfoWithRoom)
                    {
                        var room = (result as playerInfoWithRoom).room;
                        Console.WriteLine(string.Format("\tsession: {0}", room.sessionId));
                        Console.WriteLine(string.Format("\tstatus: {0}", room.status));
                        Console.WriteLine(string.Format("\ttype: {0}", room.type));
                        Console.WriteLine(string.Format("\twaiting msecs: {0}", room.nextPhaseOn));
                        Console.WriteLine(string.Format("\tactive attendees: {0}", room.attendees.Count(x => !x.Removed)));
                    }
                }
            }

        }

        private static void RefreshData()
        {
            _players.OfType<playerInfoWithRoom>().ToList().ForEach(x => GetRoomStatus(x));
            Console.WriteLine("player info refreshed");
        }

        private static playerInfo Login(playerInfo player)
        {
            var result = PostRequest<playerInfo>("/Message/Request/Login", player);

            return result;
        }

        private static bool Logout(playerInfo player)
        {
            var result = GetRequest<bool>(string.Format("/Message/Request/Logout?id={0}", player.UID));
            return result;
        }

        private static void GetPlayerRoom(playerInfo player)
        {
            var result = GetRequest<roomInfo>(string.Format("/Message/Request/GetPlayerRoom?id={0}", player.UID));
            var index = _players.FindIndex(x => x.UID == player.UID);
            if (index > -1) {
                _players[index] = new playerInfoWithRoom()
                {
                    UID = player.UID,
                    AccountName = player.AccountName,
                    isRace = player.isRace,
                    room = result
                };
            }
        }

        private static void GetRoomStatus(playerInfoWithRoom player)
        {
            if (player.room == null)
            {
                GetPlayerRoom(player);
            }
            var result = GetRequest<roomInfo>(string.Format("/Message/Request/GetRoomStatus?id={0}", player.room.sessionId));

            var index = _players.FindIndex(x => x.UID == player.UID);
            if (index > -1)
            {
                _players[index] = new playerInfoWithRoom()
                {
                    UID = player.UID,
                    AccountName = player.AccountName,
                    isRace = player.isRace,
                    room = result
                };
            }
        }

        private static T PostRequest<T>(string url, object data)
        {
            try
            {
                var result = PostRequest(url, data);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch
            {
                return default(T);
            }
        }

        private static T GetRequest<T>(string url)
        {
            try
            {
                var result = GetRequest(url);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch(Exception ex)
            {
                return default(T);
            }
        }

        private static string PostRequest(string url, object data)
        {
            string result = string.Empty;
            using (WebClient client = new WebClient()) {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                var bytes = client.UploadData(string.Format("{0}/{1}", _url.TrimEnd('/'), url.TrimStart('/')), System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data)));
                result = System.Text.Encoding.UTF8.GetString(bytes);
            }

            return result;
        }

        private static string GetRequest(string url)
        {
            string result = string.Empty;
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                result = client.DownloadString(string.Format("{0}/{1}", _url.TrimEnd('/'), url.TrimStart('/')));
            }

            return result;
        }

    }
}
