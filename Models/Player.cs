namespace PokerTest.Models
{
    public class Player
    {
        public string Name { get; private set; }
        //array
        public List<Card> Cards { get; private set; }
        public int Chips { get; private set; }
        public int CurrentBet { get;  set; }
        public string ConnectionId { get; private set; }
        // public string Action { get; private set; }

        public Player(string name, string connectionId)
        {
            ConnectionId = connectionId;
            Name = name;
            Cards = new List<Card>();
            Chips = 1000;
            CurrentBet = 0;
        }

        // scope game 
        public void AddCard(Card card)
        {
            Cards.Add(card);
        }
        public void PlaceBet(int amount)
        {
            CurrentBet = amount;
        }

    }
}