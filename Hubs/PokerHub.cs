using Microsoft.AspNetCore.SignalR;
using PokerTest;
using PokerTest.Models;

namespace SignalRChatApp.Hubs
{
    public class PokerHub : Hub
    {
        private readonly IPokerService _pokerService;
        private const string gameId = "TheOnlyOne";
        private readonly Dictionary<string, TaskCompletionSource<(string Action, int Value)>> _playerActions = new();
        private readonly Dictionary<string, CancellationTokenSource> _playerCancellationTokens = new();
        public PokerHub(IPokerService pokerService)
        {
            _pokerService = pokerService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        }

        public async Task JoinGame(string playerName)
        {
            _pokerService.AddPlayerToGame(playerName, Context.ConnectionId);

            if (_pokerService.GetTotalPlayer() == 3)
            {
                await StartGame();
            }
        }

        public async Task StartGame()
        {
            _pokerService.StartGame();

            var game = _pokerService.GetGame();
            var players = game.Players;

            var smallBlindPlayer = players[0];
            var bigBlindPlayer = players[1];
            smallBlindPlayer.PlaceBet(5);
            bigBlindPlayer.PlaceBet(10);
            game.Pot = 15;
            await Clients.Group(gameId).SendAsync("BlindsPosted", smallBlindPlayer.Name, bigBlindPlayer.Name, 0, 15);

            foreach (var player in game.Players)
                await Clients.Group(gameId).SendAsync("DealCards", game.Dealer.DealerCards);

            await Clients.Group(gameId).SendAsync("GameStarted", "Game Started");

            var currentPlayerIndex = 3 % players.Count();
            var currentPlayer = players[currentPlayerIndex];
            await Clients.Group(gameId).SendAsync("PlayerTurn",
                            currentPlayer.Name,
                            currentPlayer.Chips,
                            currentPlayer.CurrentBet,
                            players[(currentPlayerIndex + 1) % players.Count()].Name);

        }

        public async Task PreFlop(string playerName)
        {
            int playerIndex = _pokerService.GetGame().Players.FindIndex(player => player.Name == playerName);
            var currentPlayer = _pokerService.GetGame().Players[playerIndex];
            bool allEqual = _pokerService.GetGame().Players.All(x => x.CurrentBet == currentPlayer.CurrentBet);

            if (playerIndex == _pokerService.GetTotalPlayer() - 1 && allEqual)
            {
                _pokerService.GetGame().Stage = "Flop";
                await Clients.Group(gameId).SendAsync("Stage", _pokerService.GetGame().Stage);
            }
        }

        public async Task Flop(string playerName)
        {
            int playerIndex = _pokerService.GetGame().Players.FindIndex(player => player.Name == playerName);
            var currentPlayer = _pokerService.GetGame().Players[playerIndex];
            bool allEqual = _pokerService.GetGame().Players.All(x => x.CurrentBet == currentPlayer.CurrentBet);

            if (playerIndex == _pokerService.GetTotalPlayer() - 1 && allEqual)
            {
                _pokerService.GetGame().Stage = "FourthStreet";
                await Clients.Group(gameId).SendAsync("Stage", _pokerService.GetGame().Stage);
            }
        }

        public async Task FourthStreet(string playerName)
        {
            int playerIndex = _pokerService.GetGame().Players.FindIndex(player => player.Name == playerName);
            var currentPlayer = _pokerService.GetGame().Players[playerIndex];
            bool allEqual = _pokerService.GetGame().Players.All(x => x.CurrentBet == currentPlayer.CurrentBet);

            if (playerIndex == _pokerService.GetTotalPlayer() - 1 && allEqual)
            {
                _pokerService.GetGame().Stage = "FiveStreet";
                await Clients.Group(gameId).SendAsync("Stage", _pokerService.GetGame().Stage);
            }
        }
        public async Task FiveStreet(string playerName)
        {
            int playerIndex = _pokerService.GetGame().Players.FindIndex(player => player.Name == playerName);
            var currentPlayer = _pokerService.GetGame().Players[playerIndex];
            bool allEqual = _pokerService.GetGame().Players.All(x => x.CurrentBet == currentPlayer.CurrentBet);

            if (playerIndex == _pokerService.GetTotalPlayer() - 1 && allEqual)
            {
                _pokerService.GetGame().Stage = "Showdown";
                await Clients.Group(gameId).SendAsync("Stage", _pokerService.GetGame().Stage);
            }
        }
        public async Task Showdown(string playerName)
        {
            // how down
            await Clients.Group(gameId).SendAsync("Winner", "player1");
        }

        public async Task PlayerTurn(string playerName, string action, int value = 0)
        {
            await Clients.Group(gameId).SendAsync("PlayerAction", playerName, action);
            var game = _pokerService.GetGame();
            var players = game.Players;
            int playerIndex = players.FindIndex(player => player.Name == playerName);
            var currentPlayer = players[playerIndex];
            int nextIndex = (playerIndex == players.Count() - 1) ? 0 : playerIndex + 1;
            int prevIndex = (playerIndex == 0) ? players.Count() - 1 : playerIndex - 1;

            switch (action)
            {
                case "Check":
                    Console.WriteLine($"{playerName} checks.");
                    break;

                case "Call":
                    int callAmount = players[prevIndex].CurrentBet - currentPlayer.CurrentBet;
                    if (callAmount > 0)
                    {
                        currentPlayer.PlaceBet(callAmount);
                        game.Pot += callAmount;
                        Console.WriteLine($"{playerName} calls {callAmount}.");
                    }
                    else
                    {
                        Console.WriteLine($"{playerName} has nothing to call.");
                    }
                    break;

                case "Raise":
                    if (value <= 0)
                    {
                        throw new ArgumentException("Raise value must be greater than 0.");
                    }
                    currentPlayer.PlaceBet(value);
                    game.Pot += value;
                    Console.WriteLine($"{playerName} raises by {value}.");
                    break;

                case "Fold":
                    currentPlayer.Cards.Clear();
                    currentPlayer.CurrentBet = 0;
                    Console.WriteLine($"{playerName} folds.");
                    break;

                default:
                    throw new ArgumentException("Invalid action.");
            }

            await Clients.Group(gameId).SendAsync("PlayerAction", playerName, action, value);

            switch (game.Stage)
            {
                case "preflop":
                    await PreFlop(playerName);
                    break;
                case "flop":
                    await Flop(playerName);
                    break;
                case "FourthStreet":
                    await FourthStreet(playerName);
                    break;
                case "FiveStreet":
                    await FiveStreet(playerName);
                    break;
                default:
                    await Showdown(playerName);
                    break;
            }

            await Clients.Group(gameId).SendAsync("PlayerTurn",
                playerName,
                currentPlayer.Chips,
                currentPlayer.CurrentBet,
                players[nextIndex].Name);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Player? player = _pokerService.RemovePlayerFromGame(Context.ConnectionId);
            await Clients.Group(gameId).SendAsync("PlayerLeft", player?.Name);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
