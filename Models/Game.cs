namespace PokerTest.Models
{
    public class Game
    {
        public string GameId { get; private set; }
        public List<Player> Players { get; private set; }
        private Deck deck;
        public Game(string gameId)
        {
            GameId = gameId;
            Players = new List<Player>();
            deck = new Deck();
        }
        public void AddPlayerToGame(Player player)
        {
            Players.Add(player);
        }
        public Player? GetPlayer(string playerName)
        {
            foreach (var player in Players)
            {
                if (player.Name == playerName)
                {
                    return player;
                }
            }
            return null;
        }
        public void StartGame()
        {
            foreach (var player in Players)
            {
                player.AddCardToHand(deck.DrawCard());
                player.AddCardToHand(deck.DrawCard());
            }
        }
        public bool PlaceBet(Player player, int amount)
        {
            if (amount < 0 || player.Chips < amount)
                return false;
            player.PlaceBet(amount);
            return true;
        }
        public Player? RemovePlayer(string connectionId)
        {
            var player = Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player != null) Players.Remove(player);
            return player;
        }

        // public Player DetermineWinner(){
        //     return ;
        // }

    }
}