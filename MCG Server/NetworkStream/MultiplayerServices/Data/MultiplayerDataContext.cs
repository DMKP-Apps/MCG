using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Data
{
    public class MultiplayerDataContext : DbContext 
    {
        public DbSet<MultiplayerServices.Data.Entity.ClientAccount> Clients { get; set; }

        public DbSet<MultiplayerServices.Data.Entity.ClientPropertyBag> ClientPropertyBags { get; set; }

        public DbSet<MultiplayerServices.Data.Entity.Prize> Prizes { get; set; }

        public DbSet<MultiplayerServices.Data.Entity.GameStatus> GameStatuses { get; set; }

        public static void Execute(Action<MultiplayerDataContext> action)
        {
            using (MultiplayerDataContext dc = new MultiplayerDataContext())
            {
                action(dc);
            }
        }
    }
}
