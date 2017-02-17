using MultiplayerServices.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Models
{
    public class Inventory
    {
        public Inventory()
        {
            Level = 1;
            Id = Guid.NewGuid();
        }

        public Inventory(Prize prize)
        {
            Level = 1;
            Prize = prize;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Prize Prize { get; set; }

        public int Level { get; set; }

    }
}
