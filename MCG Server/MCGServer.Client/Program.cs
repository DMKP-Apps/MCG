using MCGServer.Client.Models;
using NDesk.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCGServer.Client
{
    class Program
    {
        //private static string _url = "http://localhost:8321/";
        private static string _url = "http://mcg.moebull.com/";

        private static Dictionary<string, Action> _commands = new Dictionary<string, Action>();

        private static bool _record = true;
        private static bool _autoRun = false;
        private static string _fillRoomSessionId = string.Empty;

        private static string _usernameKey = "FBC2B8D8-8F7B-41A8-99C6-9AD0E9C1806A";

        private static void runPlayerData() {
            Thread run = new Thread(new ParameterizedThreadStart((args => {

                while (isLoaded && _autoRun) {
                    
                    var stamp = DateTime.Now;
                    var items = _playerRunData.Where(y => y.timeStamp <= stamp).ToList();
                    if (items.Count > 0)
                    {
                        lock(_playerRunData)
                        {
                            _playerRunData = _playerRunData.Where(y => y.timeStamp > stamp).ToList();
                        }


                        // do stuff...
                        Console.WriteLine(string.Format("{0} items to execute.", items.Count));
                        Console.Write("> ");

                        items.OrderBy(x => x.timeStamp).ToList()
                        .ForEach(x => SendObjectData(x));
                    }

                    Thread.Sleep(200);
                    
                }

            })));
            run.Start();

            Thread query = new Thread(new ParameterizedThreadStart((args => {

                while (isLoaded)
                {
                   
                    _players.OfType<playerInfoWithRoom>().Select(x => x.room.sessionId)
                        .Distinct().ToList().ForEach(x => GetGameInfo(x));
                    
                    Thread.Sleep(1000);
                }

            })));
            query.Start();
        }

        public static string PlayerDataDir { get { return System.Configuration.ConfigurationManager.AppSettings["PlayerDataDir"]; } }

        static void parseArguments(string[] args)
        {
            var p = new OptionSet() {
                { "r|record=", "Record the none simulated player.",
                   v => _record = v.ToLower() == "true" },
                { "a|autorun=","Simulate player actions",
                    v => _autoRun = v.ToLower() == "true" },
                { "s|sessionid=","Session id of the room to fill specifically",
                    v => _fillRoomSessionId = v },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("error: ");
                Console.WriteLine(e.Message);
                return;
            }
        }

        static void Main(string[] args)
        {
            parseArguments(args);
            init();
            string cmd = string.Empty;
            if (string.IsNullOrWhiteSpace(_fillRoomSessionId))
            {
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
            }
            else
            {
                SetupAutoFillRoom();
            }

            _players.ForEach(x => Logout(x));
            isLoaded = false;
        }

        static void SetupAutoFillRoom()
        {
            try
            {
                var room = GetRequest<roomInfo>(string.Format("/Message/Request/GetRoomStatus?id={0}", _fillRoomSessionId));
                while (room != null && room.status == RoomStatus.New)
                {

                    var player = new playerInfo()
                    {
                        UID = Guid.NewGuid().ToString(),
                        AccountName = _usernameKey,
                        isRace = false,
                        sessionId = _fillRoomSessionId
                    };

                    var autoFillPlayer = Login(player);
                    if (autoFillPlayer == null)
                    {   // something has changed and the room is no longer available... break out.
                        break;
                    }

                    Console.WriteLine(string.Format("User '{0}' is logged in.", autoFillPlayer.AccountName));
                    _players.Add(autoFillPlayer);
                    GetPlayerRoom(autoFillPlayer);

                    Random rand = new Random();

                    var sleep = rand.Next(Convert.ToInt32(room.nextPhaseOn * 0.2), Convert.ToInt32(room.nextPhaseOn * 0.8));
                    /*if (sleep < 1000) {
                        sleep = 1000;
                    }*/

                    Thread.Sleep(Convert.ToInt32(sleep));
                    room = GetRequest<roomInfo>(string.Format("/Message/Request/GetRoomStatus?id={0}", _fillRoomSessionId));
                }

                // continue to loop as long as the _players has items and the room is not closed.
                while (_players.OfType<playerInfoWithRoom>().Count() > 0 && !_players.OfType<playerInfoWithRoom>().Any(x => x.room == null || x.room.status == RoomStatus.Closed))
                {
                    Thread.Sleep(2000);
                }

            }
            catch { }
            
        }

        static List<playerInfo> _players = new List<playerInfo>();
        static bool isLoaded = false;

        private static void init() {
            _commands.Add("login", PromptLogin);
            _commands.Add("logout", PromptLogout);
            _commands.Add("pinfo", PromptPlayerDetails);
            _commands.Add("refresh", RefreshData);
            //_commands.Add("run", PromptPlayerRun);
            _commands.Add("rec", () => _record = true);
            _commands.Add("recoff", () => _record = false);
            _commands.Add("run", () => _autoRun = true);
            _commands.Add("runoff", () => _autoRun = false);

            isLoaded = true;

            runPlayerData();

        }

        private static void PromptLogin()
        {
            var accountName = _usernameKey;
            Console.Write("[1] is race (Y/N): ");
            var isRace = Console.ReadKey();
            Console.WriteLine("");
            
            var race = isRace.KeyChar.ToString().ToUpper() != "N";

            var player = new playerInfo()
            {
                UID = Guid.NewGuid().ToString(),
                AccountName = accountName,
                isRace = race
            };

            player = Login(player);
            Console.WriteLine(string.Format("User '{0}' is logged in.", player.AccountName));
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

        private static List<NetworkObjectData> _playerRunData = new List<NetworkObjectData>();

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

        private static void SendObjectData(NetworkObjectData data)
        {
            var result = PostRequest<playerInfo>("/Message/Request/SaveObjectData", new {
                objectId = data.objectId,
                accName = data.accName,
                sessionId = data.sessionId,
                holeId = data.holeId,
                isRace = data.isRace,
                Ready = true,
                cannon_position_x = data.cannon_position_x,
                cannon_position_y = data.cannon_position_y,
                cannon_position_z = data.cannon_position_z,
                cannon_rotation_x = data.cannon_rotation_x,
                cannon_rotation_y = data.cannon_rotation_y,
                cannon_rotation_z = data.cannon_rotation_z,
                currentBullet = data.currentBullet,
                fire = data.fire,
                fire_accurracy = data.fire_accurracy,
                fire_power = data.fire_power,
                fire_torque = data.fire_torque,
                fire_turn = data.fire_turn,
                holeComplete = data.holeComplete,
                root_position_x = data.root_position_x,
                root_position_y = data.root_position_y,
                root_position_z = data.root_position_z,
                root_rotation_x = data.root_rotation_x,
                root_rotation_y = data.root_rotation_y,
                root_rotation_z = data.root_rotation_z,
                stroke = data.stroke,
                waitMilliseconds = data.waitMilliseconds
            });

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

        private static roomInfo GetRoomStatus(string sessionId)
        {
            var result = GetRequest<roomInfo>(string.Format("/Message/Request/GetRoomStatus?id={0}", sessionId));
            return result;
        }

        static List<NetworkPlayerData> _otherplayerData = new List<NetworkPlayerData>();

        private static void GetGameInfo(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) {
                return;
            }

            var room = GetRoomStatus(sessionId);
            // update each of the players in this room
            lock(_players)
            {
                _players.OfType<playerInfoWithRoom>().Where(x => x.room != null && x.room.sessionId == sessionId)
                    .ToList().ForEach(x => x.room = room);

                var checkforPlayersList = room.attendees.Where(x => !x.Removed).Join(_players.OfType<playerInfoWithRoom>().Where(x => x.room.sessionId == sessionId), x => x.UID, y => y.UID, (x, y) => y).ToList();
                if (checkforPlayersList.Count == room.attendees.Count(x => !x.Removed))
                {
                    checkforPlayersList.ForEach(x => {
                        Logout(x);
                        var index = _players.FindIndex(y => y.UID == x.UID);
                        if (index > -1) {
                            _players.RemoveAt(index);
                        }
                    });
                    
                }

                if (!room.attendees.Any(a => _players.Any(x => a.UID != x.UID)))
                {
                    _players = new List<playerInfo>();
                    return;
                }
                //if(_players.Any(x => ))
            }
            if (room == null)
            {
                Console.WriteLine(string.Format("ALERT: Room '{0}', does not exist.", sessionId));
                return;
            }

            if (room.status == RoomStatus.Waiting || room.status == RoomStatus.HoleCompleted)
            {
                room.attendees.Join(_players, x => x.UID, y => y.UID, (x, y) => new { playerNumber = x.playerNumber, player = y })
                    .Where(x => x.player.DataLoaded).ToList().ForEach(x =>
                    {
                        var player = _players.OfType<playerInfo>().FirstOrDefault(p => p.UID == x.player.UID);
                        if (player == null)
                            return;
                        lock(_playerRunData)
                        {
                            _playerRunData = _playerRunData.Where(p => p.objectId != player.UID).ToList();
                        }
                        player.DataLoaded = false;

                    
                    });

                Console.WriteLine(string.Format("Room: {0}, Status: {1}", room.sessionId, room.status));
            }

            if (room.status == RoomStatus.InProgress)
            {
                room.attendees.Join(_players, x => x.UID, y => y.UID, (x, y) => new { playerNumber = x.playerNumber, player = y })
                    .Where(x => !x.player.DataLoaded).ToList().ForEach(x =>
                    {
                        var dir = string.Format("{0}\\{1}\\Hole{2}\\Player{3}", PlayerDataDir.TrimEnd('\\'), room.course, room.currentHole, x.playerNumber);
                        if (System.IO.Directory.Exists(dir))
                        {
                            var player = _players.OfType<playerInfo>().FirstOrDefault(p => p.UID == x.player.UID);
                            if (player == null)
                                return;

                            var jsonFile = System.IO.Directory.GetFiles(dir, "*.json").OrderBy(f => Guid.NewGuid())
                            .FirstOrDefault();


                            string data = string.Empty;
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(jsonFile))
                            {
                                data = sr.ReadToEnd();
                            }

                            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NetworkObjectData>>(data);
                            items.ForEach(i =>
                            {
                                i.objectId = player.UID;
                                i.accName = player.AccountName;
                                i.sessionId = room.sessionId;
                            });
                            player.DataLoaded = true;
                            lock (_playerRunData)
                            {
                                _playerRunData.AddRange(items);
                            }

                        }
                    });
            }

            if (!_record)
            {
                return;
            }

            var result = GetRequest<List<NetworkPlayerData>>(string.Format("/Message/Request/GetGameInfoBySessionId?id={0}", sessionId));
            if (result != null)
            {
                _otherplayerData.AddRange(result.Where(x => !_players.Any(y => y.UID == x.objectId)));
            }
            if (_otherplayerData.Count > 0)
            {
                

                room.attendees.Select(x => new {
                    playerNumber = x.playerNumber,
                    UID = x.UID
                })
                .Join(_otherplayerData, x => x.UID, y => y.objectId, (y, x) => new {
                        playerNumber = y.playerNumber,
                        objectId = y.UID,
                        holeId = x.holeId,
                        isRace = x.isRace,
                        uniqueId = x.uniqueId,
                        cannon_position_x = x.cannon_position_x,
                        cannon_position_y = x.cannon_position_y,
                        cannon_position_z = x.cannon_position_z,
                        cannon_rotation_x = x.cannon_rotation_x,
                        cannon_rotation_y = x.cannon_rotation_y,
                        cannon_rotation_z = x.cannon_rotation_z,
                        currentBullet = x.currentBullet,
                        fire = x.fire,
                        fire_accurracy = x.fire_accurracy,
                        fire_power = x.fire_power,
                        fire_torque = x.fire_torque,
                        fire_turn = x.fire_turn,
                        holeComplete = x.holeComplete,
                        root_position_x = x.root_position_x,
                        root_position_y = x.root_position_y,
                        root_position_z = x.root_position_z,
                        root_rotation_x = x.root_rotation_x,
                        root_rotation_y = x.root_rotation_y,
                        root_rotation_z = x.root_rotation_z,
                        stroke = x.stroke,
                        timeStamp = x.timeStamp,
                        waitMilliseconds = x.fire ? 1000 : x.waitMilliseconds
                })
                .GroupBy(x => x.playerNumber)
                .ToList().ForEach(k => {

                    var fromTime = k.OrderBy(x => x.timeStamp).Select(x => x.timeStamp).FirstOrDefault();

                    var items = k.GroupBy(x => x.uniqueId).Select(x => x.FirstOrDefault()).Select(x => new
                    {
                        holeId = x.holeId,
                        isRace = x.isRace,
                        cannon_position_x = x.cannon_position_x,
                        cannon_position_y = x.cannon_position_y,
                        cannon_position_z = x.cannon_position_z,
                        cannon_rotation_x = x.cannon_rotation_x,
                        cannon_rotation_y = x.cannon_rotation_y,
                        cannon_rotation_z = x.cannon_rotation_z,
                        currentBullet = x.currentBullet,
                        fire = x.fire,
                        fire_accurracy = x.fire_accurracy,
                        fire_power = x.fire_power,
                        fire_torque = x.fire_torque,
                        fire_turn = x.fire_turn,
                        holeComplete = x.holeComplete,
                        root_position_x = x.root_position_x,
                        root_position_y = x.root_position_y,
                        root_position_z = x.root_position_z,
                        root_rotation_x = x.root_rotation_x,
                        root_rotation_y = x.root_rotation_y,
                        root_rotation_z = x.root_rotation_z,
                        stroke = x.stroke,
                        timeElasped = x.timeStamp.Subtract(fromTime).TotalMilliseconds,
                        waitMilliseconds = x.fire ? 1000 : x.waitMilliseconds
                    }).ToList();

                    var filename = string.Format("{0}", PlayerDataDir.TrimEnd('\\'));
                    if (!System.IO.Directory.Exists(filename)) {
                        System.IO.Directory.CreateDirectory(filename);
                    }

                    filename = string.Format("{0}\\{1}", PlayerDataDir.TrimEnd('\\'), room.course);
                    if (!System.IO.Directory.Exists(filename))
                    {
                        System.IO.Directory.CreateDirectory(filename);
                    }

                    filename = string.Format("{0}\\{1}\\Hole{2}", PlayerDataDir.TrimEnd('\\'), room.course, room.currentHole);
                    if (!System.IO.Directory.Exists(filename))
                    {
                        System.IO.Directory.CreateDirectory(filename);
                    }

                    filename = string.Format("{0}\\{1}\\Hole{2}\\Player{3}", PlayerDataDir.TrimEnd('\\'), room.course, room.currentHole, k.Key);
                    if (!System.IO.Directory.Exists(filename))
                    {
                        System.IO.Directory.CreateDirectory(filename);
                    }

                    filename = string.Format("{0}\\{1}\\Hole{2}\\Player{3}\\{4}.json", PlayerDataDir.TrimEnd('\\'), room.course, room.currentHole, k.Key, sessionId);

                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, false))
                    {
                        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(items));
                    }

                });

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
