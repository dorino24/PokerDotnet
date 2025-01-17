using Microsoft.AspNetCore.SignalR;
using PokerTest;
using PokerTest.Models;
using PokerTest.Models.Enums;
using PokerTest.Services.Interface;

namespace SignalRChatApp.Hubs
{
    public class PokerHub : Hub
    {
        private readonly IPokerService _pokerService;
        private const string gameId = "TheOnlyOne";
        private static int _readyNextRoundCount = 0;
        private int _smallBlindBet = 5;
        private int _bigBlindBet = 10;
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
            _pokerService.CreateGame(_smallBlindBet, _bigBlindBet, gameId);
            bool isAdded = _pokerService.AddPlayerToGame(playerName, Context.ConnectionId, gameId);
            await Clients.Client(Context.ConnectionId).SendAsync("PlayerJoined",  isAdded ? "PlayerName Joined": "PlayerName already exist", isAdded);
            if (_pokerService.GetTotalPlayer(gameId) == 3) await StartGame();
        }

        public async Task StartGame()
        {
            _pokerService.StartGame(gameId);
            Game? game = _pokerService.GetGame(gameId);
            List<Player> players = game!.Players;
            Player currentPlayer = players[1 % players.Count()];
            game.Stage = Stage.PREFLOP;

            foreach (Player player in game.Players)
                await Clients.Group(gameId).SendAsync("InitialGame", player.Name, player.Chips, player.Cards.Select(card => card.ToString()).ToArray(), player.CurrentBet, game.Dealer.DealerCards.Select(card => card.ToString()).ToArray());

            await Clients.Group(gameId).SendAsync("GameStarted", "Game Started", game.Players.Select(player => player.Name).ToList());
            await Clients.Group(gameId).SendAsync("PlayerTurn",
                            currentPlayer.Name,
                            currentPlayer.Chips,
                            currentPlayer.CurrentBet,
                            players[((1 % players.Count()) + 1) % players.Count()].Name);
        }


        public async Task PlayerTurn(string playerName, string action, int value = 0)
        {
            List<Player> players = _pokerService.GetGame(gameId)!.Players;
            int playerIndex = players.FindIndex(player => player.Name == playerName);
            Player currentPlayer = players[playerIndex];
            int nextIndex = (playerIndex == players.Count - 1) ? 0 : playerIndex + 1;
            int prevIndex = (playerIndex == 0) ? players.Count() - 1 : playerIndex - 1;

            while (players[nextIndex].IsFold)
                nextIndex = (nextIndex == players.Count - 1) ? 0 : nextIndex + 1;

            while (players[prevIndex].IsFold)
                prevIndex = (prevIndex == 0) ? players.Count() - 1 : prevIndex - 1;

            _pokerService.PlayerAction(playerName, action, value, gameId);

            await Clients.Group(gameId).SendAsync("PlayerAction", playerName, action, value);

            bool onlyOneNotFolded = players.Count(player => !player.IsFold) == 1;

            if (onlyOneNotFolded)
            {
                players[nextIndex].Chips += _pokerService.GetGame(gameId)!.Pot;
                await Clients.Group(gameId).SendAsync("Winner", players[nextIndex].Name, "All Fold", players[nextIndex].Chips);
                return;
            }
            await CheckForNextStage(playerName);

            if (_pokerService.GetGame(gameId)!.Stage == Stage.SHOWDOWN) return;

            if (players[playerIndex].IsFold) currentPlayer = players[prevIndex];

            await Clients.Group(gameId).SendAsync("PlayerTurn",
                currentPlayer.Name,
                currentPlayer.Chips,
                currentPlayer.CurrentBet,
                players[nextIndex].Name);
        }
        public async Task CheckForNextStage(string playerName)
        {
            Game game = _pokerService.GetGame(gameId)!;
            List<Player> players = game.Players;

            if (_pokerService.CheckForNextStage(playerName, gameId)) await Clients.Group(gameId).SendAsync("InitalNewStage", game.Stage, game.Pot);

            if (game.Stage == Stage.SHOWDOWN)
            {
                foreach (var player in players)
                    await Clients.Group(gameId).SendAsync("ShowAllCards", player.Name, player.Cards.Select(card => card.ToString()).ToArray());

                var winnerDTO = game.DetermineWinner();
                await Clients.Group(gameId).SendAsync("Winner", winnerDTO.PlayerWinner.Name, winnerDTO.CardRanking, winnerDTO.PlayerWinner.Chips);
            }
        }

        public async Task NextRound()
        {
            _readyNextRoundCount++;
            if (_readyNextRoundCount == _pokerService.GetGame(gameId)!.Players.Count)
            {
                _pokerService.GetGame(gameId)!.NextRound();
                await StartGame();
                _readyNextRoundCount = 0;
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Player? player = _pokerService.RemovePlayerFromGame(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("PlayerLeft", player?.Name);
            await base.OnDisconnectedAsync(exception);
        }
    }

}
