using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Data.Entity
{
    public enum GameStates
    { 
        New = 1,
        InProgress = 2,
        Finished = 3
    }

    public class GameStatus : Entity
    {
        public GameStatus()
            : base()
        {
            GameState = GameStates.New;
        }

        [Key, Column("ID", TypeName = "nvarchar"), MaxLength(50)]
        public string Id { get; set; }

        [Column("PropertyValue", TypeName = "ntext")]
        public string Value { get; set; }

        public GameStates GameState { get; set; }
    }
}
