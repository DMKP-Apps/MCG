using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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


[Serializable]
public class RoomAttendee
{
    public RoomAttendee()
    {
        //rankings = new Dictionary<int, int>();
        Removed = false;
    }
    public string UID;
    public string AccountName;
    public int playerNumber;
    public int position;
    //public Dictionary<int, int> rankings;
    public bool Removed;

}

[Serializable]
public class roomInfo
{
    public roomInfo()
    {
        attendees = new List<RoomAttendee>();
    }
    public string sessionId;
    public int maxAttendance;
    public int minAttendance;
    public double created;

    public GameType type;
    public string course;
    public int currentHole;
    public double nextPhaseOn;

    public RoomStatus status;

    public List<RoomAttendee> attendees;

    public List<RoomAttendee> getActiveAttendees()
    {
        return attendees.Where(x => !x.Removed).ToList();
    }
}