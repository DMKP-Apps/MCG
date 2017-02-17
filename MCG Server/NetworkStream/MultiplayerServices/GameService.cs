using MultiplayerServices.Data;
using MultiplayerServices.Managers;
using MultiplayerServices.Messages;
using MultiplayerServices.Models;
using SocketServer.Library;
using SocketServer.Library.Attributes;
using SocketServer.Library.Requests;
using SocketServer.Library.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerServices
{
    [SocketRequestService]
    public class GameService
    {
        List<RoomSmall> _rooms = new List<RoomSmall>();
        List<Game> _games = new List<Game>();

        public int PlayerTimeout { get { return 30; } }

        private static readonly object _lock = new object();
        private static void Lock(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }

        public GameService()
        {

        }

        [ClientConnectMethod]
        public void ClientConnect(string clientId)
        {
            // nothing.
        }

        [ClientDisconnectMethod]
        public void ClientDisconnect(string clientId)
        {
            ClientManager.Clients.SetClientOffline(clientId);
        }

        [SocketRequestMethod]
        public SocketResponse<string> Logon(object data)
        {
            var request = SocketRequest.ConvertRequest<string>(data);
            var response = SocketResponse.GetResponse<string, string>(request);
            response.Data = ClientManager.Clients[request.ClientId].Name;
            ClientManager.Clients.UpdateLastClientRequest(request.ClientId);
            return response;
        }

        [SocketRequestMethod]
        public SocketResponse<RoomSmall> Join(object data)
        {
            var request = SocketRequest.ConvertRequest<Inventory>(data);
            var response = SocketResponse.GetResponse<RoomSmall, Inventory>(request);
            ClientManager.Clients.UpdateLastClientRequest(request.ClientId);

            int index = -1;
            var isInGame = false;
            var isRejoin = false;
            // check if the player exists within a room already...
            
            Client c = ClientManager.Clients[request.ClientId];
            c.InventoryItem = request.Data;

            Lock(() => { 
                index = _games.FindIndex(x => x.Players.Any(y => y.ID == request.ClientId));
                isInGame = index > -1;
            });
            if(index < 0)
                Lock(() => index = _rooms.FindIndex(x => x.GetClients().Any(y => y.ID == request.ClientId)));

            isRejoin = index > -1;


            if (index < 0)
            {
                Lock(() =>
                {
                    index = _rooms.FindIndex(x => x.GetEmtpySeats() > 0 && x.Level == request.Data.Level);
                    if (index < 0)
                    {
                        response.Data = new RoomSmall(request.Data.Level, c);
                        _rooms.Add(response.Data);
                    }
                    else
                    {
                        _rooms[index].AddClient(c);
                        response.Data = _rooms[index];
                        index = _rooms.Count - 1;
                    }
                });
            }

            if (!isInGame && !isRejoin)
            {
                string message = string.Format("User '{0}' has joined the room.", request.Data);
                response.Data.GetClients().Where(x => x != null && x.ID != request.ClientId)
                    .ToList().ForEach(x => MessageManager.Add(x.ID, new ChatAlertMessage(message)));

                if (response.Data.GetEmtpySeats() == 0)
                {
                    var gameIdx = -1;
                    // game is on 
                    Game newgame = new Game()
                    {
                        Players = response.Data.GetClients().Select(x => new Player() { ID = x.ID, LastValue = new Spin(), Name = x.Name, Postion = 0, IsConnect = false, InventoryItem = x.InventoryItem })
                            .OrderBy(x => Guid.NewGuid()).ToList(),
                        RoomId = response.Data.ID,
                        CurrentPlayerIndex = 0
                    };
                    Lock(() =>
                    {
                        _rooms.RemoveAt(index);
                        _games.Add(newgame);
                        gameIdx = _games.Count - 1;
                    });

                    newgame.Players
                        .ToList().ForEach(x => MessageManager.Add(x.ID, new ChatAlertMessage("Game is on!")));

                    UpdateGamePlayers(gameIdx);
                }
            }
            else if (isInGame)
            {
                response.Data = new RoomSmall(request.Data.Level)
                {
                    ID = _games[index].RoomId,
                };

            }
            else
            {
                response.Data = _rooms[index];
            }

            return response;
        }

        protected void UpdateGamePlayers(int gameIdx)
        {
            _games[gameIdx].WinningPlayerIndex = _games[gameIdx].Players.FindIndex(x => x.Postion >= _games[gameIdx].WinPostion);
            if (_games[gameIdx].WinningPlayerIndex > -1)
            {
                Lock(() =>
                {
                    //_games[gameIdx].Players[_games[gameIdx].WinningPlayerIndex].
                    List<Inventory> winningItems = new List<Inventory>();

                    MultiplayerDataContext.Execute((dc) =>
                    {
                        _games[gameIdx].Players.Where(x => x.ID != _games[gameIdx].Players[_games[gameIdx].WinningPlayerIndex].ID)
                            .ToList()
                            .ForEach((x) =>
                            {
                                var entity = dc.ClientPropertyBags.Where(y => y.AccountId == x.ID && y.Key == "profile").FirstOrDefault();
                                var profile = entity.Get<Profile>();
                                var lostItem = profile.Inventory.Where(i => i.Id == x.InventoryItem.Id).FirstOrDefault();
                                profile.Inventory = profile.Inventory.Where(i => i.Id != x.InventoryItem.Id).ToList();
                                entity.Set(profile);
                                winningItems.Add(lostItem);
                            });

                        _games[gameIdx].Players.Where(x => x.ID == _games[gameIdx].Players[_games[gameIdx].WinningPlayerIndex].ID)
                            .ToList()
                            .ForEach((x) =>
                            {
                                var entity = dc.ClientPropertyBags.Where(y => y.AccountId == x.ID && y.Key == "profile").FirstOrDefault();
                                var profile = entity.Get<Profile>();
                                var levelUpItem = profile.Inventory.Where(i => i.Id == x.InventoryItem.Id).FirstOrDefault();
                                levelUpItem.Level++;
                                winningItems.Add(levelUpItem);

                                profile.Inventory = profile.Inventory.Where(i => i.Id != x.InventoryItem.Id).Union(winningItems).ToList();
                                entity.Set(profile);
                            });

                        dc.SaveChanges();
                    });

                    _games[gameIdx].Log();

                    _games[gameIdx].Players
                        .ToList().ForEach(x => MessageManager.Add(x.ID, new ChatAlertMessage(string.Format("Game over! Player '{0}' wins!", _games[gameIdx].Players[_games[gameIdx].WinningPlayerIndex].Name))));

                    _games[gameIdx].Players
                        .ToList().ForEach(x => MessageManager.Add(x.ID, new GameOverMessage()));

                    _games[gameIdx].Players
                        .ToList().ForEach(x => MessageManager.Add(x.ID, new RequestActionMessage("Profile", "GetProfile", x.Name)));

                    _games.RemoveAt(gameIdx);
                });



            }
            else
            {

                MessageManager.Add(_games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].ID, new RequestActionMessage("Game", "Spin", _games[gameIdx].RoomId));
                MessageManager.Add(_games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].ID, new ChatAlertMessage("Your turn to roll!"));
                _games[gameIdx].Players.Where(x => x.ID != _games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].ID)
                        .ToList().ForEach(x => MessageManager.Add(x.ID, new ChatAlertMessage(string.Format("Player '{0}' is rolling...", _games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].Name))));

                _games[gameIdx].Players
                       .ToList().ForEach(x => MessageManager.Add(x.ID, new CurrentPlayerMessage(_games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].ID, _games[gameIdx].CurrentPlayerIndex)));

                _games[gameIdx].Log();

                //CurrentPlayerMessage
                Thread timer = new Thread(new ParameterizedThreadStart((args) =>
                {
                    var currentGameIdx = (int)args;
                    var currentPlayerIndex = _games[currentGameIdx].CurrentPlayerIndex;
                    DateTime startTime = DateTime.Now;
                    bool outofTime = false;
                    while (_games[currentGameIdx].CurrentPlayerIndex == currentPlayerIndex && !outofTime)
                    {
                        var diff = DateTime.Now - startTime;
                        outofTime = diff.TotalSeconds > this.PlayerTimeout;
                        if (outofTime)
                            break;
                        Thread.Sleep(100);
                    }

                    if (_games[currentGameIdx].CurrentPlayerIndex == currentPlayerIndex && outofTime)
                    {
                        MessageManager.Add(_games[currentGameIdx].Players[currentPlayerIndex].ID, new ChatAlertMessage("Your turn timed out!"));
                        _games[currentGameIdx].Players.Where(x => x.ID != _games[currentGameIdx].Players[currentPlayerIndex].ID)
                            .ToList().ForEach(x => MessageManager.Add(x.ID, new ChatAlertMessage(string.Format("!Player '{0}' has timed out", _games[currentGameIdx].Players[_games[currentGameIdx].CurrentPlayerIndex].Name))));

                        currentPlayerIndex++;
                        if (currentPlayerIndex >= _games[currentGameIdx].Players.Count)
                            currentPlayerIndex = 0;
                        _games[currentGameIdx].CurrentPlayerIndex = currentPlayerIndex;
                        UpdateGamePlayers(currentGameIdx);
                    }

                }));

                timer.Start(gameIdx);
            }
           
        }

        
        [SocketRequestMethod]
        public SocketResponse<int> Spin(object data)
        {
            var request = SocketRequest.ConvertRequest<Spin>(data);
            var response = SocketResponse.GetResponse<int, Spin>(request);
            ClientManager.Clients.UpdateLastClientRequest(request.ClientId);

            int gameIdx = 0;
            Lock(() => gameIdx = _games.FindIndex(x => x.RoomId == request.Data.RoomId));

            if (_games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].ID != request.ClientId)
                throw new Exception("Oopsies! It's not your turn");

            _games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].LastValue = request.Data;
            _games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].Postion += request.Data.Stay ? request.Data.GetPositions() : 0;
            response.Data = _games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].Postion;

            _games[gameIdx].Players.Where(x => x.ID != _games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].ID)
                    .ToList().ForEach(x => MessageManager.Add(x.ID, new ChatAlertMessage(string.Format("Player '{0}' rolled a {1}", _games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].Name, request.Data))));

            if (request.Data.Stay)
            {
                MessageManager.Add(_games[gameIdx].Players[_games[gameIdx].CurrentPlayerIndex].ID, new ChatAlertMessage(string.Format("You move {0} positions! ", response.Data)));

                _games[gameIdx].CurrentPlayerIndex++;
                if (_games[gameIdx].CurrentPlayerIndex >= _games[gameIdx].Players.Count)
                    _games[gameIdx].CurrentPlayerIndex = 0;

                UpdateGamePlayers(gameIdx);
            }

            return response;
        }

        [SocketRequestMethod]
        public SocketResponse<IEnumerable<Client>> GetClients(object data)
        {
            var request = SocketRequest.ConvertRequest<string>(data);
            var response = SocketResponse.GetResponse<IEnumerable<Client>, string>(request);
            ClientManager.Clients.UpdateLastClientRequest(request.ClientId);

            response.Data = _rooms.Where(x => x.ID == request.Data).SelectMany(x => x.GetClients().Select(p => new Client() { ID = p.ID, Name = p.Name }))
                .Union(_games.Where(x => x.RoomId == request.Data).SelectMany(x => x.Players.Select(p => new Client() { ID = p.ID, Name = p.Name })))
                .GroupBy(x => x.ID)
                .Select(x => x.FirstOrDefault())
                .ToList();

            return response;
        }

        

    }
}
