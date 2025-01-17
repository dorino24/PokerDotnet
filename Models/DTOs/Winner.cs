namespace PokerTest.Models.DTOs
{
    public class WinnerDTO
    {
        public required Player PlayerWinner { get; set; }
        public string CardRanking { get; set;} = string.Empty;
    }
}