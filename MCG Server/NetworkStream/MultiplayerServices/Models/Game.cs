using MultiplayerServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketServer.Library;
using System.Threading;

namespace MultiplayerServices.Models
{

    public class Player : Client
    {
        public bool IsConnect { get; set; }
        public Spin LastValue { get; set; }
        public int Postion { get; set; }

    }

    public class Game
    {
        public Game()
        {
            WinPostion = 15;
            WinningPlayerIndex = -1;
        }

        public List<Player> Players { get; set; }
        public string RoomId { get; set; }

        public int CurrentPlayerIndex { get; set; }

        public int WinningPlayerIndex { get; set; }

        public int WinPostion { get; set; }

        public void Log()
        {
            Thread loggin = new Thread(new ParameterizedThreadStart((args) => {

                var obj = args as Game;

                MultiplayerDataContext.Execute((dc) => {
                    var gamestatus = dc.GameStatuses.FirstOrDefault(x => x.Id == obj.RoomId);
                    if (gamestatus == null)
                    {
                        gamestatus = new Data.Entity.GameStatus() { Id = obj.RoomId, Value = obj.ToJsonString() };
                        dc.GameStatuses.Add(gamestatus);
                    }
                    else
                    {
                        if (obj.WinningPlayerIndex > -1) 
                            gamestatus.GameState = Data.Entity.GameStates.Finished;
                        else
                            gamestatus.GameState = Data.Entity.GameStates.InProgress;

                        gamestatus.Value = obj.ToJsonString();
                        gamestatus.ModifiedOn = DateTime.Now.ToUniversalTime();
                    }
                    dc.SaveChanges();
                });
            }));
            loggin.Start(this);

        }


    }
}
