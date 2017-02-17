using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Models
{
    public abstract class Room
    { //
        public Room(int size, int level)
        {
            this.ID = Guid.NewGuid().ToString("N");
            this._size = size;
            _level = level;
            this._clients = new List<Client>();
            for (int i = 0; i < _size; i++)
            {
                _clients.Add(null);
            }
        }
        public string ID { get; set; }
        private int _size = 0;

        private readonly object _lock = new object();

        private List<Client> _clients = new List<Client>();
        private bool _isFull = false;
        private int _level = 1;

        public int Level { get { return _level; } }

        public int GetEmtpySeats()
        {
            if (_isFull) return 0;
            var cnt = _clients.Count(x => x == null);
            _isFull = cnt == 0;
            return cnt;            
        }

        public IEnumerable<Client> GetClients()
        {
            lock (_lock)
            {
                return _clients.Where(x => x != null);
            }

        }

        public bool AddClient(Client client)

        {
            if (_isFull) return false;
            lock (_lock)
            {
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (_clients[i] == null)
                    {
                        _clients[i] = client;
                        break;
                    }
                }
                return true;
            }

        }

    }

    public class RoomSmall : Room
    {
        public RoomSmall(int level)
            : base(4, level)
        {
        }

        public RoomSmall(int level, Client client)
            : base(4, level)
        {
            this.AddClient(client);
        }
    }

    public class RoomLarge : Room
    {
        public RoomLarge(int level)
            : base(6, level)
        {
        }

        public RoomLarge(int level, Client client)
            : base(6, level)
        {
            this.AddClient(client);
        }
    } 
}
