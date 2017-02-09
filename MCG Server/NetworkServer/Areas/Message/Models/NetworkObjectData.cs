using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetworkServer.Areas.Message.Models
{
    public enum RoomStatus
    {
        None = 0,
        New = 1,
        Waiting = 2,
        InProgress = 3,
        Closed = 4,
        HoleCompleted = 5

    }

    public enum GameType
    {
        Traditional,
        Race

    }

    
    public class RoomAttendee
    {
        public RoomAttendee() {
            rankings = new Dictionary<int, int>();
            Removed = false;
        }
        public string UID { get; set; }
        public string AccountName { get; set; }
        public int playerNumber { get; set; }
        public int position { get; set; }
        public Dictionary<int, int> rankings { get; set; }
        public bool Removed { get; set; }

    }

    public class Room
    {
        public Room()
        {
            created = DateTime.Now;
            lastRequestTime = DateTime.Now;
            type = GameType.Race;
            
            sessionId = Guid.NewGuid().ToString();
            maxAttendance = 4;
            minAttendance = 2;

            _workFlow.Add(RoomStatus.None, () => { nextPhaseOn = null; });
            _workFlow.Add(RoomStatus.New, () => {

                Random rand = new Random();
                _randomStartCount = rand.Next(15, 30);

                nextPhaseOn = DateTime.Now.AddSeconds(_randomStartCount);
                currentHole = 0;
            });
            _workFlow.Add(RoomStatus.Waiting, () => {
                nextPhaseOn = DateTime.Now.AddSeconds(10);
            });
            _workFlow.Add(RoomStatus.InProgress, () => {
                nextPhaseOn = null;
                currentHole++;
            });
            _workFlow.Add(RoomStatus.HoleCompleted, () => {
                nextPhaseOn = DateTime.Now.AddSeconds(10);
            });
            _workFlow.Add(RoomStatus.Closed, () => {
                nextPhaseOn = null;
            });

            status = RoomStatus.New;
        }

        public string sessionId { get; set; }
        public int maxAttendance { get; set; }
        public int minAttendance { get; set; }
        public DateTime created { get; set; }
        public DateTime lastRequestTime { get; set; }
        
        public GameType type { get; set; }
        public string course { get; set; }
        public int currentHole { get; set; }
        public DateTime? nextPhaseOn { get; set; }
        private int? _processId = null;
        private int _randomStartCount = 0;

        private ConcurrentDictionary<string, RoomAttendee> _attendees = new ConcurrentDictionary<string, RoomAttendee>();
        public ConcurrentDictionary<string, RoomAttendee> attendees
        {
            get
            {
                if (_attendees == null) {
                    _attendees = new ConcurrentDictionary<string, RoomAttendee>();
                }
                return _attendees;
            }
            set
            {
                if (value == null)
                {
                    _attendees = new ConcurrentDictionary<string, RoomAttendee>();
                }
                else
                {
                    _attendees = value;
                }
            }
        }

        private Dictionary<RoomStatus, Action> _workFlow = new Dictionary<RoomStatus, Action>();

        private RoomStatus _status = RoomStatus.None;
        public RoomStatus status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value != _status)
                {
                    _status = value;
                    if (_workFlow.ContainsKey(_status))
                    {
                        _workFlow[_status]();
                    }
                }
            }
        }

        public void PlayerCompletedHole(string UID)
        {
            var ranking = _attendees.Where(x => x.Value.rankings.ContainsKey(currentHole))
                .Select(x => x.Value.rankings[currentHole]).OrderByDescending(x => x)
                .FirstOrDefault();
            ranking++;

            if (_attendees.ContainsKey(UID) && !_attendees[UID].Removed) {
                if (!_attendees[UID].rankings.ContainsKey(currentHole))
                {
                    _attendees[UID].rankings[currentHole] = ranking;
                }
            }

            // now check to see how many player need to completed the hole that are currently active.
            var numberOfIncompleted = _attendees.Where(x => !x.Value.Removed).Count(x => !x.Value.rankings.ContainsKey(currentHole));
            if (numberOfIncompleted == 1)
            {
                // update the only player with the whole completed...
                var playerId = _attendees.Where(x => !x.Value.Removed && !x.Value.rankings.ContainsKey(currentHole)).Select(x => x.Key).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(playerId))
                {
                    PlayerCompletedHole(playerId);
                }
            }
            else if (numberOfIncompleted == 0 && status == RoomStatus.InProgress)
            {
                status = RoomStatus.HoleCompleted;
            }

        }

        protected string exePath { get { return System.Configuration.ConfigurationManager.AppSettings["autoPlayerExe"]; } }

        //protected bool exeInBackground { get { return System.Configuration.ConfigurationManager.AppSettings["autoPlayerExe_background"] == "true"; } }

        public void ProcessNextPhase()
        {
            if (status == RoomStatus.New && attendees.Count < minAttendance && !_processId.HasValue)
            {   // no users available... close room

                // run the auto similate client.
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    _processId = -1;
                }
                else
                {
                    var p = System.Diagnostics.Process.Start(exePath, string.Format("-r false -a true -s \"{0}\"", this.sessionId));
                    _processId = p.Id;
                    //p.
                }

                nextPhaseOn = DateTime.Now.AddSeconds(60 - _randomStartCount);
            }
            else if (status == RoomStatus.New && attendees.Count < minAttendance && _processId.HasValue)
            {   // no users available... close room
                
                status = RoomStatus.Closed;
            }
            else if (status == RoomStatus.New && attendees.Count >= minAttendance)
            {   status = RoomStatus.Waiting;
            }
            else if (status == RoomStatus.Waiting && attendees.Count(x => !x.Value.Removed) >= minAttendance)
            {   status = RoomStatus.InProgress;
            }
            else if (status == RoomStatus.HoleCompleted && attendees.Count(x => !x.Value.Removed) >= minAttendance)
            {   status = RoomStatus.InProgress;
            }
            else if (attendees.Count(x => !x.Value.Removed) < minAttendance && 
                (status != RoomStatus.New && status != RoomStatus.Closed))
            {   // no users available... close room
                status = RoomStatus.Closed;
            }
        }

    }


    public class PlayerLoginModel
    {
        public string UID { get; set; }
        public string AccountName { get; set; }
        public bool isRace { get; set; }
        public string sessionId { get; set; }

    }

    //public class PlayerLoginModelWithSession : PlayerLoginModel
    //{
        

    //}
    public abstract class NetworkData
    {
        public NetworkData()
        {
            timeStamp = DateTime.Now.ToUniversalTime();
            uniqueId = Guid.NewGuid().ToString();
            type = this.GetType().Name;
        }
        public string type { get; set; }
        public string uniqueId { get; set; }
        public string Key { get { return objectId; } }
        public DateTime timeStamp { get; set; }
        public string objectId { get; set; }
        public string accName { get; set; }
        public string sessionId { get; set; }
        public bool isRace { get; set; }

    }

    public class NetworkWaitingData : NetworkData
    {
        public NetworkWaitingData() : base()
        {
            Ready = false;
        }

        public bool Ready { get; set; }
    }

    public class NetworkObjectData : NetworkWaitingData
    {
        public NetworkObjectData() : base()
        {
            fire = false;
            playerType = "default";
            waitMilliseconds = 0;
        }
        
        public string playerType { get; set; }
        public string objectName { get; set; }
        public string holeId { get; set; }

        public float root_position_x { get; set; }
        public float root_position_y { get; set; }
        public float root_position_z { get; set; }

        public float root_rotation_x { get; set; }
        public float root_rotation_y { get; set; }
        public float root_rotation_z { get; set; }

        public float cannon_position_x { get; set; }
        public float cannon_position_y { get; set; }
        public float cannon_position_z { get; set; }

        public float cannon_rotation_x { get; set; }
        public float cannon_rotation_y { get; set; }
        public float cannon_rotation_z { get; set; }

        public int stroke { get; set; }
        public bool fire { get; set; }
        public float fire_torque { get; set; }
        public float fire_turn { get; set; }
        public float fire_power { get; set; }
        public float fire_accurracy { get; set; }

        public double waitMilliseconds { get; set; }

        public int currentBullet { get; set; }
        public bool holeComplete { get; set; }

        public void UpdateWaitMillieseconds() {
            if (waitMilliseconds <= 0) {
                return; // do nothing
            }
            var currentDateTime = DateTime.Now.ToUniversalTime();
            var milliseconds = currentDateTime.Subtract(timeStamp).TotalMilliseconds;

            waitMilliseconds = waitMilliseconds - milliseconds;
            if (waitMilliseconds < 0) {
                waitMilliseconds = 0;
            }

        }

    }

   
    
}