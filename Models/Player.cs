namespace PokerTest.Models
{
    public class Player
    {
        public string Name { get; private set; }
        //array
        public List<Card> Cards { get; private set; }
        public int Chips { get; set; }
        public int CurrentBet { get; set; }
        public string ConnectionId { get; private set; }
        public bool IsFold { get; set; }

        public Player(string name, string connectionId)
        {
            ConnectionId = connectionId;
            Name = name;
            Cards = new List<Card>();
            Chips = 1000;
            CurrentBet = 0;
            IsFold = false;
        }
        public void AddCard(Card card)
        {
            Cards.Add(card);
        }
        public void PlaceBet(int amount)
        {
            Chips -= amount - CurrentBet;
            CurrentBet = amount;
        }
        public void Fold()
        {
            IsFold = true;
        }
        public void NextRound()
        {
            Cards.Clear();
            CurrentBet = 0;
            IsFold = false;
        }
    }
}