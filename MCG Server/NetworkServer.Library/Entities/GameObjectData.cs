using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServer.Library.Entities
{
    [Table("GameObjects")]
    public class GameObjectData
    {
        public GameObjectData()
        {
            Id = Guid.NewGuid();
            timeStamp = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        [Column(TypeName = "NVARCHAR"), StringLength(128)]
        public string holeId { get; set; }

        public DateTime timeStamp { get; set; }

        [Column(TypeName = "NVARCHAR"), StringLength(256)]
        public string objectId { get; set; }

        [Column(TypeName = "NVARCHAR"), StringLength(256)]
        public string objectName { get; set; }

        [Column(TypeName = "NVARCHAR"), StringLength(128)]
        public string type { get; set; }

        [Column(TypeName = "NVARCHAR"), StringLength(4000)]
        public string data { get; set; }

        
        


        //public string position;
        //public string rotation;
    }
}
