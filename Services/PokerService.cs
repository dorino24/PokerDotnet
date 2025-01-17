
using PokerTest.Models;
using PokerTest.Models.DTOs;
using PokerTest.Models.Enums;
using PokerTest.Services.Interface;
namespace PokerTest.Services;
public class PokerService : IPokerService
{
    private readonly List<Game> _games;
    public PokerService()
    {
        _games = new List<Game>();
    }
    public void CreateGame(int smallBlindBet, int bigBlindBet, string gameId)
    {
        Game? existingGame = _games.FirstOrDefault(g => g.GameId == gameId);
        if (existingGame == null)
        {
            Game newGame = new Game(gameId, smallBlindBet, bigBlindBet);
            _games.Add(newGame);
        }
    }
    public void StartGame(string gameId)
    {
        Game? existingGame = _games.FirstOrDefault(g => g.GameId == gameId);
        if (existingGame == null) return;
        existingGame.StartGame();
    }
    public bool AddPlayerToGame(string playerName, string connectionId, string gameId)
    {
        Game? game = _games.FirstOrDefault(g => g.GameId == gameId);
        if (game != null)
        {
            Player player = new Player(playerName, connectionId);
            bool isAdded = game.AddPlayerToGame(player);
            return isAdded;
        }
        return false;
    }
    public int GetTotalPlayer(string gameId)
    {
        Game? game = _games.FirstOrDefault(g => g.GameId == gameId);
        return game != null ? game.Players.Count() : 0;
    }

    public Player? RemovePlayerFromGame(string connectionId, string gameId)
    {
        Game? game = _games.FirstOrDefault(g => g.GameId == gameId);
        return game != null ? game.RemovePlayer(connectionId) : null;
    }

    public Game? GetGame(string gameId)
    {
        Game? game = _games.FirstOrDefault(g => g.GameId == gameId);
        return game != null ? game : null;
    }

    public WinnerDTO DetermineWinner(string gameId)
    {
        Game? game = _games.FirstOrDefault(g => g.GameId == gameId);
        return game!.DetermineWinner();
    }
    public void PlayerAction(string playerName, string action, int value, string gameId)
    {
        Game? game = GetGame(gameId);
        if (game == null) return;
        List<Player> players = game.Players;
        int playerIndex = players.FindIndex(player => player.Name == playerName);
        Player currentPlayer = players[playerIndex];
        int nextIndex = (playerIndex == players.Count() - 1) ? 0 : playerIndex + 1;
        int prevIndex = (playerIndex == 0) ? players.Count() - 1 : playerIndex - 1;
        while (players[prevIndex].IsFold)
            prevIndex = (prevIndex == 0) ? players.Count() - 1 : prevIndex - 1;

        while (players[nextIndex].IsFold)
            nextIndex = (nextIndex == players.Count - 1) ? 0 : nextIndex + 1;

        switch (action)
        {
            case "Check":
                break;

            case "Call":
                int callAmount = players[prevIndex].CurrentBet - currentPlayer.CurrentBet;
                if (callAmount > 0)
                {
                    currentPlayer.PlaceBet(players[prevIndex].CurrentBet);
                    game.Pot += callAmount;
                }
                break;

            case "Raise":
                currentPlayer.PlaceBet(value);
                game.Pot += value;
                break;

            case "Fold":
                currentPlayer.Fold();
                currentPlayer.CurrentBet = 0;
                break;

            default:
                throw new ArgumentException("Invalid action.");
        }
    }
    public bool CheckForNextStage(string playerName, string gameId)
    {
        Game? game = GetGame(gameId);
        if (game == null) return false;

        List<Player> players = game.Players;
        int playerIndex = players.FindIndex(player => player.Name == playerName);
        Player currentPlayer = players[playerIndex];
        bool allEqual = players
                            .Where(x => !x.IsFold)
                            .All(x => x.CurrentBet == currentPlayer.CurrentBet);
        int lastPlayerIndex = 1;
        while (players[lastPlayerIndex].IsFold)
        {
            lastPlayerIndex = (lastPlayerIndex + 1) % players.Count;
        }
        if (lastPlayerIndex >= players.Count)
            return false;

        Player lastPlayerCheck = players[lastPlayerIndex];
        if (currentPlayer == null || !allEqual || currentPlayer != lastPlayerCheck) return false;

        game.Stage = game.Stage switch
        {
            Stage.PREFLOP => Stage.FLOP,
            Stage.FLOP => Stage.TURN,
            Stage.TURN => Stage.RIVER,
            _ => Stage.SHOWDOWN
        };
        players.ForEach(player => player.CurrentBet = 0);

        return true;
    }
}