namespace PokerTest.Models
{
    public class Dealer 
    {
        public List<Card> DealerCards { get; private set; }
        public Dealer()
        {
            DealerCards = new List<Card>();
        }
        public void AddCard(Card card)
        {
             DealerCards.Add(card);
        }
    }
}
