namespace PokerTest.Models
{
    public class Player
    {
        public string Name { get; private set; }
        public List<Card> Hand { get; private set; }
        public int Chips { get; private set; }
        public string ConnectionId {get; private set;}

        public Player(string name,string connectionId)
        {
            ConnectionId = connectionId;
            Name = name;
            Hand = new List<Card>();
            Chips = 1000;
        }

        public void AddCardToHand(Card card)
        {
            Hand.Add(card);
        }
        public void PlaceBet(int amount)
        {
            if (amount > Chips)
            {
                throw new InvalidOperationException("Not enough");
            }
            Chips -= amount;
        }

    }
}