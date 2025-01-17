namespace PokerTest.Models.DTOs
{
    public class CheckRanking
    {
        public int MaxValue { get; set; }
        public required int[] Kickers { get; set; }
        public int Ranking { get; set; }
    }
}