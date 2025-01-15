using PokerTest.Models.Enums;

namespace PokerTest.Models
{
    public class Game
    {
        public string GameId { get; private set; }
        //use array
        public List<Player> Players { get; private set; }
        private Deck deck;
        public Dealer Dealer;
        public int Pot { get; set; }
        public Stage Stage { get; set; }
        public Game(string gameId)
        {
            GameId = gameId;
            Players = new List<Player>();
            deck = new Deck();
            Dealer = new Dealer();
            Stage = Stage.PREFLOP;
        }
        public void AddPlayerToGame(Player player)
        {
            //validation kalo dah ada ato hashSet
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
            //Action untuk animasi 
            for (int i = 0; i < 5; i++)
            {
                Dealer.AddCard(deck.DrawCard());
            }
            foreach (var player in Players)
            {
                player.AddCard(deck.DrawCard());
                player.AddCard(deck.DrawCard());
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

        public (Player, string) DetermineWinner()
        {
            var highestRanking = 0;
            var highestMaxValue = 0;
            var highestKickers = Array.Empty<int>();
            Player? winner = null;

            foreach (var player in Players)
            {
                foreach (var dealerCard in Dealer.DealerCards)
                {
                    player.AddCard(dealerCard);
                }

                var pokerRanking = PokerEvaluator.EvaluateHand(player);
                System.Console.WriteLine((Ranking)pokerRanking.Ranking);
                if (pokerRanking.Ranking > highestRanking)
                {
                    highestRanking = pokerRanking.Ranking;
                    highestMaxValue = pokerRanking.MaxValue;
                    highestKickers = pokerRanking.Kickers;
                    winner = player;
                }
                else if (pokerRanking.Ranking == highestRanking)
                {
                    if (pokerRanking.MaxValue > highestMaxValue)
                    {
                        highestMaxValue = pokerRanking.MaxValue;
                        highestKickers = pokerRanking.Kickers;
                        winner = player;
                    }
                    else if (pokerRanking.MaxValue == highestMaxValue)
                    {
                        for (int i = 0; i < pokerRanking.Kickers.Length; i++)
                        {
                            if (i >= highestKickers.Length || pokerRanking.Kickers[i] > highestKickers[i])
                            {
                                highestKickers = pokerRanking.Kickers;
                                winner = player;
                                break;
                            }
                            else if (pokerRanking.Kickers[i] < highestKickers[i])
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return (winner!, ((Ranking)highestRanking).ToString());
        }
    }
}