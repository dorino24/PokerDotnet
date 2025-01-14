using PokerTest.Models;

namespace PokerTest
{
    public interface IPokerService
    {
        public void StartGame();
        public Game GetGame();
        public void AddPlayerToGame(string playerName, string connectionId);
        public int GetTotalPlayer();
        public Player? RemovePlayerFromGame(string connectionId);
    }
}