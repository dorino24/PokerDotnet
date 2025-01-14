using PokerTest;
using PokerTest.Models;

public class PokerService : IPokerService
{
    private readonly Game _game;
    public PokerService()
    {
        _game = new Game("TheOnlyOne");
    }
    public void StartGame()
    {
        _game.StartGame();
    }
    public void AddPlayerToGame(string playerName, string connectionId)
    {
        Player player = new Player(playerName, connectionId);
        _game.AddPlayerToGame(player);
    }
    public int GetTotalPlayer()
    {
        return _game.Players.Count();
    }

    public Player? RemovePlayerFromGame(string connectionId)
    {
        return _game.RemovePlayer(connectionId);
    }

    public Game GetGame()
    {
        return _game;

    }


}