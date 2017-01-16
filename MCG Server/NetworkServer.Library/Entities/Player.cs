using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServer.Library.Entities
{
    [Table("Players")]
    public class Player
    {
        [Key]
        public string UID { get; set; }

        [Column(TypeName = "NVARCHAR"), StringLength(256), Index(IsUnique=true)]
        public string AccountName { get; set; }

        //public DateTime? AccountName { get; set; }
    }
}
