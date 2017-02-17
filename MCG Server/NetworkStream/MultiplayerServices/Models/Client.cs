using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Models
{
    public class Client
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public Inventory InventoryItem { get; set; }
    }
}
