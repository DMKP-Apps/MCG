using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NetworkServer.Areas.Message.Models
{
    public interface INetworkDataRepository
    {
        Room GetAvailableRoomForNewLogin(PlayerLoginModel player);
        List<Room> GetActiveRooms();
        bool RemovePlayerFromRoom(string id);
        Room GetRoomByKey(string key);

        NetworkData Add(NetworkData item);
        IEnumerable<NetworkData> GetAll();
        NetworkData Find(string key);
        NetworkData Remove(string key);

        IEnumerable<TNetworkData> GetAll<TNetworkData>() where TNetworkData : NetworkData;

        void Update(NetworkData item);
        bool ShutDown { get; set; }
    }

    public class NetworkDataRepository : INetworkDataRepository
    {
        private static ConcurrentDictionary<string, NetworkData> _messages =
              new ConcurrentDictionary<string, NetworkData>();

        private static ConcurrentDictionary<string, Room> _rooms =
              new ConcurrentDictionary<string, Room>();

        public bool ShutDown { get; set; }
        private DateTime _lastCheckTime = DateTime.Now;
        public NetworkDataRepository()
        {
            ShutDown = false;
            var thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((args) => {
                while (!ShutDown)
                {
                    if (DateTime.Now.Subtract(_lastCheckTime).TotalMinutes > 5)
                    {
                        _lastCheckTime = DateTime.Now;
                        ClearStaleSessions();
                    }

                    ProcessRoomWorkflow();

                    System.Threading.Thread.Sleep(500);
                }
                
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        public Room GetAvailableRoomForNewLogin(PlayerLoginModel player)
        {
            Room room = null;

            // check if the currend login is already attending another room.
            var existingRooms = _rooms.Where(x => x.Value.attendees.ContainsKey(player.UID)).Select(x => x.Key).ToList();
            // close the account for the existing rooms
            existingRooms.ForEach(x => {
                if (_rooms[x].status == RoomStatus.New)
                {
                    // remove them from the room completely
                    RoomAttendee attendee;
                    _rooms[x].attendees.TryRemove(player.UID, out attendee);
                    if (_rooms[x].attendees.Count == 0) {
                        _rooms[x].status = RoomStatus.Closed;
                    }
                }
                else
                {
                    _rooms[x].attendees[player.UID].Removed = true;
                    
                }
            });

            // get an available room based on the players isRace value
            var key = _rooms.Where(x => x.Value.status == RoomStatus.New && x.Value.maxAttendance > x.Value.attendees.Count && ((player.isRace && x.Value.type == GameType.Race) || (!player.isRace && x.Value.type == GameType.Traditional)))
                .OrderBy(x => x.Value.created).Select(x => x.Key).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(key))
            {   // no room available... create new room and add the player.
                key = Guid.NewGuid().ToString();
                _rooms[key] = new Room()
                {
                    sessionId = key,
                    status = RoomStatus.New,
                    type = player.isRace ? GameType.Race : GameType.Traditional,
                    maxAttendance = player.isRace ? 4 : 2,
                };
            }

            _rooms[key].attendees[player.UID] = new RoomAttendee()
            {
                UID = player.UID,
                AccountName = player.AccountName,
            };

            if (_rooms[key].attendees.Count >= _rooms[key].maxAttendance)
            {
                _rooms[key].status = RoomStatus.Waiting;
            }

            room = _rooms[key];


            return room;
        }

        public List<Room> GetActiveRooms()
        {
            return _rooms.Where(x => x.Value.status != RoomStatus.Closed)
                .Select(x => x.Value).ToList();
        }

        public Room GetRoomByKey(string key)
        {
            Room room = null;
            if (!_rooms.TryGetValue(key, out room))
                return null;
            return room;
        }

        public bool RemovePlayerFromRoom(string id)
        {
            var item = Find(id);
            if (item == null)
            {
                _rooms.Where(x => x.Value.attendees.ContainsKey(id))
                    .ToList().ForEach(x =>
                    {
                        x.Value.attendees[id].Removed = true;
                        if (x.Value.attendees.Count(y => !y.Value.Removed) < 2) {
                            x.Value.status = RoomStatus.Closed;
                        }
                    });
            }
            else if (_rooms.ContainsKey(item.sessionId) && _rooms[item.sessionId].attendees.ContainsKey(id))
            {
                _rooms[item.sessionId].attendees[id].Removed = true;

                if (_rooms[item.sessionId].attendees.Count(y => !y.Value.Removed) < 2)
                {
                    _rooms[item.sessionId].status = RoomStatus.Closed;
                }
            }

            return true;
        }

        private void ProcessRoomWorkflow()
        {
            _rooms.Where(x => x.Value.nextPhaseOn.HasValue && x.Value.nextPhaseOn.Value < DateTime.Now).ToList()
                .ForEach(x => {
                    x.Value.ProcessNextPhase();
                });
        }

        private void ClearStaleSessions()
        {
            var queryDate = DateTime.Now.ToUniversalTime();
            var oldMessages = _messages.Values.Where(x => queryDate.Subtract(x.timeStamp).TotalMinutes > 10).ToList();
            oldMessages.ForEach(x => {
                var uid = x.Key;
                var sessionId = x.sessionId;

                if (_rooms.ContainsKey(sessionId)) {
                    var containsKey = _rooms[sessionId].attendees.ContainsKey(uid);
                    if (containsKey) {
                        _rooms[sessionId].attendees[uid].Removed = true;
                    }

                    if (!_rooms[sessionId].attendees.Any(a => !a.Value.Removed) || _rooms[sessionId].status == RoomStatus.Closed)
                    {   // room is closed.
                        Room item;
                        _rooms.TryRemove(sessionId, out item);
                    }
                }

                Remove(x.Key);
            });
            
        }

        public IEnumerable<TNetworkData> GetAll<TNetworkData>()
            where TNetworkData : NetworkData
        {
            var result = _messages.Values.OfType<TNetworkData>();
            return result;
        }

        public IEnumerable<NetworkData> GetAll()
        {
            return _messages.Values;
        }

        public NetworkData Add(NetworkData item)
        {
            item.timeStamp = DateTime.Now.ToUniversalTime();
            item.type = item.GetType().Name;
            if (string.IsNullOrWhiteSpace(item.uniqueId))
            {
                item.uniqueId = Guid.NewGuid().ToString();
            }
            _messages[item.Key] = item;

            return item;
        }

        public NetworkData Find(string key)
        {
            NetworkData item;
            _messages.TryGetValue(key, out item);
            return item;
        }

        public NetworkData Remove(string key)
        {
            NetworkData item;
            _messages.TryRemove(key, out item);
            return item;
        }

        public void Update(NetworkData item)
        {
            item.timeStamp = DateTime.Now.ToUniversalTime();
            item.type = item.GetType().Name;
            if (string.IsNullOrWhiteSpace(item.uniqueId))
            {
                item.uniqueId = Guid.NewGuid().ToString();
            }
            _messages[item.Key] = item;
        }
    }
}