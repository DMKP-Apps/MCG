using NetworkServer.Library.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServer.Library
{
    public class MCGDbContext : DbContext
    {
       
        public DbSet<Player> Players { set; get; }
        public DbSet<GameObjectData> GameObjectData { set; get; }
        

        /* NOTE: 
         *   Setting "Default" to base class helps us when working migration commands on Package Manager Console.
         *   But it may cause problems when working Migrate.exe of EF. If you will apply migrations on command line, do not
         *   pass connection string name to base classes. ABP works either way.
         */
        public MCGDbContext()
            : base("Default")
        {

        }

        /* NOTE:
         *   This constructor is used by ABP to pass connection string defined in TTSwapDataModule.PreInitialize.
         *   Notice that, actually you will not directly create an instance of TTSwapDbContext since ABP automatically handles it.
         */
        public MCGDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }

        //This constructor is used in tests
        public MCGDbContext(DbConnection connection)
            : base(connection, true)
        {

        }
    }
}
