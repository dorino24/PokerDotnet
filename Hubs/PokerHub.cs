using Microsoft.AspNetCore.SignalR;
using PokerTest;
using PokerTest.Models;
using PokerTest.Models.Enums;

namespace SignalRChatApp.Hubs
{
    public class PokerHub : Hub
    {
        private readonly IPokerService _pokerService;
        private const string gameId = "TheOnlyOne";
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

            var listPlayer = new List<string>();
            foreach (var player in game.Players)
            {
                listPlayer.Add(player.Name);
                var playerCardsAsString = player.Cards.Select(card => card.ToString()).ToArray();
                var dealerCardsAsString = game.Dealer.DealerCards.Select(card => card.ToString()).ToArray();
                await Clients.Group(gameId).SendAsync("InitialGame", player.Name, player.Chips, playerCardsAsString, player.CurrentBet, dealerCardsAsString);
            }

            await Clients.Group(gameId).SendAsync("GameStarted", "Game Started", listPlayer);

            var currentPlayerIndex = 1 % players.Count();
            var currentPlayer = players[currentPlayerIndex];
            await Clients.Group(gameId).SendAsync("PlayerTurn",
                            currentPlayer.Name,
                            currentPlayer.Chips,
                            currentPlayer.CurrentBet,
                            players[(currentPlayerIndex + 1) % players.Count()].Name);
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
                        currentPlayer.PlaceBet(players[prevIndex].CurrentBet);
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
                case Stage.PREFLOP:
                    await PreFlop(playerName);
                    break;
                case Stage.FLOP:
                    await Flop(playerName);
                    break;
                case Stage.TURN:
                    await Turn(playerName);
                    break;
                case Stage.RIVER:
                    await River(playerName);
                    break;
                default:
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
        public async Task PreFlop(string playerName)
        {
            var game = _pokerService.GetGame();
            var players = game.Players;
            int playerIndex = players.FindIndex(player => player.Name == playerName);
            var currentPlayer = players[playerIndex];
            bool allEqual = players.All(x => x.CurrentBet == currentPlayer.CurrentBet);

            if (allEqual && currentPlayer == players[1])
            {
                _pokerService.GetGame().Stage = Stage.FLOP;
                foreach (var player in players)
                {
                    player.CurrentBet = 0;
                }
                await Clients.Group(gameId).SendAsync("InitalNewStage", game.Stage, game.Pot);
            }
        }

        public async Task Flop(string playerName)
        {
            var game = _pokerService.GetGame();
            var players = game.Players;
            int playerIndex = players.FindIndex(player => player.Name == playerName);
            var currentPlayer = players[playerIndex];
            bool allEqual = players.All(x => x.CurrentBet == currentPlayer.CurrentBet);
            if (currentPlayer == players[1] && allEqual)
            {
                game.Stage = Stage.TURN;
                foreach (var player in players)
                {
                    player.CurrentBet = 0;
                }
                await Clients.Group(gameId).SendAsync("InitalNewStage", game.Stage, game.Pot);
            }
        }


        public async Task Turn(string playerName)
        {
            var game = _pokerService.GetGame();
            var players = game.Players;
            int playerIndex = players.FindIndex(player => player.Name == playerName);
            var currentPlayer = players[playerIndex];
            bool allEqual = players.All(x => x.CurrentBet == currentPlayer.CurrentBet);
            if (currentPlayer == players[1] && allEqual)
            {
                game.Stage = Stage.RIVER;
                foreach (var player in players)
                {
                    player.CurrentBet = 0;
                }
                await Clients.Group(gameId).SendAsync("InitalNewStage", game.Stage, game.Pot);
            }
        }
        public async Task River(string playerName)
        {
            var game = _pokerService.GetGame();
            var players = game.Players;
            int playerIndex = players.FindIndex(player => player.Name == playerName);
            var currentPlayer = players[playerIndex];
            bool allEqual = players.All(x => x.CurrentBet == currentPlayer.CurrentBet);

            if (currentPlayer == players[1] && allEqual)
            {
                _pokerService.GetGame().Stage = Stage.SHOWDOWN;
                foreach (var player in players)
                {
                    player.CurrentBet = 0;
                    var playerCardsAsString = player.Cards.Select(card => card.ToString()).ToArray();
                    await Clients.Group(gameId).SendAsync("ShowAllCards", player.Name, playerCardsAsString);
                }
                await Clients.Group(gameId).SendAsync("InitalNewStage", game.Stage, game.Pot);
                // winnner winner chicken dinner

                (Player winnerPlayer, string highestRanking) = game.DetermineWinner();
                await Clients.Group(gameId).SendAsync("Winner", winnerPlayer.Name, highestRanking);
            }
        }
    }

}
