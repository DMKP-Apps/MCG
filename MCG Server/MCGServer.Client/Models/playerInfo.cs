using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCGServer.Client.Models
{
    class playerInfo
    {
        public string UID { get; set; }
        public string AccountName { get; set; }
        public bool isRace { get; set; }
    }

    public enum RoomStatus
    {
        None = 0,
        New = 1,
        Waiting = 2,
        InProgress = 3,
        Closed = 4

    }

    public enum GameType
    {
        Traditional,
        Race

    }

    public class RoomAttendee
    {
        public RoomAttendee()
        {
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

    class roomInfo
    {
        public roomInfo()
        {
            attendees = new List<RoomAttendee>();
        }
        public string sessionId { get; set; }
        public int maxAttendance { get; set; }
        public int minAttendance { get; set; }
        public double created { get; set; }

        public GameType type { get; set; }
        public string course { get; set; }
        public int currentHole { get; set; }
        public double nextPhaseOn { get; set; }

        public RoomStatus status { get; set; }

        public List<RoomAttendee> attendees { get; set; }
    }

    class playerInfoWithRoom : playerInfo
    {
        public playerInfoWithRoom() : base()
        {
            room = null;
        }

        public roomInfo room { get; set; }

    }
}
