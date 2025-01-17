using PokerTest.Models;

namespace PokerTest.Services.Interface
{
    public interface IPokerService
    {
        public void CreateGame(int smallBlindBet, int bigBlindBet, string gameId);
        public void StartGame( string gameId);
        public Game? GetGame(string gameId);
        public bool AddPlayerToGame(string playerName, string connectionId, string gameId);
        public int GetTotalPlayer(string gameId);
        public Player? RemovePlayerFromGame(string connectionId, string gameId);
        public bool CheckForNextStage(string playerName, string gameId);
        public void PlayerAction(string playerName, string action, int value, string gameId);
    }
}