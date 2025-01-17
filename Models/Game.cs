using PokerTest.Models.DTOs;
using PokerTest.Models.Enums;

namespace PokerTest.Models
{
    public class Game
    {
        public string GameId { get; private set; }
        public List<Player> Players { get; private set; }
        public int Pot { get; set; }
        public Stage Stage { get; set; }
        public Dealer Dealer { get; private set; }
        private Deck _deck;
        private readonly int _bigBlindBet;
        private readonly int _smallBlindBet;

        public Game(string gameId, int smallBlindbet, int bigBlindBet)
        {
            GameId = gameId;
            Players = new List<Player>();
            _deck = new Deck();
            Dealer = new Dealer();
            _bigBlindBet = bigBlindBet;
            _smallBlindBet = smallBlindbet;

        }
        public bool AddPlayerToGame(Player player)
        {
            if (!Players.Any(p => p.Name == player.Name))
            {
                Players.Add(player);
                return true;
            }
            return false;

        }
        public Player? GetPlayer(string playerName)
        {
            foreach (var player in Players)
                if (player.Name == playerName) return player;

            return null;
        }
        public void StartGame()
        {
            for (int i = 0; i < 5; i++)
                Dealer.AddCard(_deck.DrawCard());

            foreach (var player in Players)
            {
                player.AddCard(_deck.DrawCard());
                player.AddCard(_deck.DrawCard());
            }

            Players[0].PlaceBet(_smallBlindBet);
            Players[1].PlaceBet(_bigBlindBet);
            Pot = _smallBlindBet + _bigBlindBet;
        }
        public bool PlaceBet(Player player, int amount)
        {
            if (amount < 0 || player.Chips < amount) return false;
            player.PlaceBet(amount);
            return true;
        }
        public Player? RemovePlayer(string connectionId)
        {
            var player = Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player != null) Players.Remove(player);
            return player;
        }

        public void NextRound()
        {
            Pot = 0;
            if (_deck.Cards.Count < (5 + 2 * Players.Count))
            {
                _deck = new Deck();
            }
            Dealer.DealerCards.Clear();
            foreach (var player in Players)
            {
                player.NextRound();
            }
        }

        public WinnerDTO DetermineWinner()
        {
            int highestRanking = 0;
            int highestMaxValue = 0;
            int[] highestKickers = Array.Empty<int>();
            Player? winner = null;

            foreach (var player in Players)
            {
                if (player.IsFold != true)
                {
                    foreach (var dealerCard in Dealer.DealerCards)
                    {
                        player.AddCard(dealerCard);
                    }

                    var pokerRanking = PokerEvaluator.EvaluateHand(player);
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
            }
            winner!.Chips += Pot;

            return new WinnerDTO { PlayerWinner = winner, CardRanking = ((Ranking)highestRanking).ToString() };
        }
    }
}