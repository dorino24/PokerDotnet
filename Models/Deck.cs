using PokerTest.Models.Enums;

namespace PokerTest.Models
{
    public class Deck
    {
        private List<Card> cards;
        public Deck()
        {
            //ctor ada param untuk bisa custom ienumerable 

            cards = new List<Card>();
            Initialization();
        }
        private void Initialization()
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card(suit, rank));
                }
            }
            ShuffleDeck();
        }
        public void ShuffleDeck()
        {
            var random = new Random();
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }
        public Card DrawCard()
        {
            Card card = cards.First();
            cards.RemoveAt(0);
            return card;
        }

    }
}