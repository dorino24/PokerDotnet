using Microsoft.AspNetCore.SignalR;
using PokerTest;
using PokerTest.Models;

namespace SignalRChatApp.Hubs
{
    public class PokerHub : Hub
    {
        private readonly IPokerService _pokerService ;
        private const string GameId = "TheOnlyOne";
        public PokerHub(IPokerService pokerService)
        {
            _pokerService = pokerService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, GameId);
        }

        public async Task JoinGame(string playerName)
        {
            _pokerService.AddPlayerToGame(playerName, Context.ConnectionId);
            await Clients.Group(GameId).SendAsync("PlayerJoined", playerName);  
            if (_pokerService.GetTotalPlayer() == 2)
            {
                _pokerService.StartGame();
                _pokerService.GetGame();
                
                await Clients.Group(GameId).SendAsync("GameStarted", "Game Started");
                foreach (var player in _pokerService.GetGame().Players)
                {
                    var playerHand = new List<string> { player.Hand[0].ToString(), player.Hand[1].ToString() };
                    await Clients.Group(GameId).SendAsync("PlayerHand", player.Name, playerHand);
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Player? player = _pokerService.RemovePlayerFromGame(Context.ConnectionId);
            await Clients.Group(GameId).SendAsync("PlayerLeft", player?.Name);
            await base.OnDisconnectedAsync(exception);
        }

        // public async Task PlayHand(string playerName, string action, int value = 0)
        // {
        //     await Clients.All.SendAsync("PlayerAction", playerName, action);

        //     // await Clients.All.SendAsync("PlayerTurn", _playerName[_currentPlay]);
        // }
    }
}