using PokerTest.Models.Enums;

namespace PokerTest.Models
{
    public class Card
    {
        public Suit Suit {get;private set;}
        public Rank Rank {get;private set;}

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
        public override string ToString()
        {
            return $"{Rank}_{Suit}";
        }
    }
}