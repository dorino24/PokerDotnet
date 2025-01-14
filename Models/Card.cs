namespace PokerTest.Models
{
    public class Card
    {
        //better enum 
        public string Suit {get;private set;}
        public string Value {get;private set;}

        public Card(string suit, string value)
        {
            Suit = suit;
            Value = value;
        }
        public override string ToString()
        {
            return $"{Value}_{Suit}";
        }
    }
}