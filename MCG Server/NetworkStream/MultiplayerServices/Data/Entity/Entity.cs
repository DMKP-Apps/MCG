using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Data.Entity
{
    public class Entity
    {
        public Entity()
        {
            this.CreatedOn = DateTime.Now.ToUniversalTime();
            this.ModifiedOn = DateTime.Now.ToUniversalTime();
        }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
