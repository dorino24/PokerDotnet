using PokerTest.Models.Enums;

namespace PokerTest.Models
{
    public class Deck
    {
        public List<Card> Cards;
        public Deck()
        {
            Cards = new List<Card>();
            Initialization();
        }
        private void Initialization()
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    Cards.Add(new Card(suit, rank));
                }
            }
            ShuffleDeck();
        }
        public void ShuffleDeck()
        {
            var random = new Random();
            for (int i = Cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = Cards[i];
                Cards[i] = Cards[j];
                Cards[j] = temp;
            }
        }
        public Card DrawCard()
        {
            Card card = Cards.First();
            Cards.RemoveAt(0);
            return card;
        }

    }
}