using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Data.Entity
{
    public enum PrizeType
    { 
        Character = 1,
        Perk = 2
    }

    public class Prize : Entity
    {
        public Prize() : base() { }
        [Key,
        DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity),
        Column("ID", TypeName = "int")]
        public int Id { get; set; }

        [Column("Name", TypeName = "nvarchar"), MaxLength(128),
        Index()]
        public string Name { get; set; }

        [Column("ShortName", TypeName = "nvarchar"), MaxLength(25),
        Index()]
        public string ShortName { get; set; }

        public PrizeType Type { get; set; }



    }
}
