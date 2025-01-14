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
            // enum
            string[] suits = ["hearts", "diamonds", "clubs", "spades"];
            string[] ranks = ["2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"];

            // can custom initialization 
            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
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