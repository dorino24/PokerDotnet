using PokerTest.Models.DTOs;
using PokerTest.Models.Enums;

namespace PokerTest.Models
{
    public class PokerEvaluator
    {
        public static CheckRanking EvaluateHand(Player hand)
        {
            int[] cards = hand.Cards.Select(c => (int)c.Rank).OrderBy(v => v).ToArray();
            if (IsRoyalFlush(hand)) return new CheckRanking { MaxValue = 14, Kickers = [], Ranking = (int)Ranking.ROYAL_FLUSH };
            if (IsStraightFlush(hand).check) return new CheckRanking { MaxValue = IsStraightFlush(hand).max, Kickers = [], Ranking = (int)Ranking.STRAIGHT_FLUSH };
            if (IsFourOfAKind(hand).check) return new CheckRanking { MaxValue = IsFourOfAKind(hand).max, Kickers = GetKickers(cards, IsFourOfAKind(hand).max), Ranking = (int)Ranking.FOUR_OF_A_KIND };
            if (IsFullHouse(hand).check) return new CheckRanking { MaxValue = IsFullHouse(hand).maxThree, Kickers = GetKickers(cards, IsFullHouse(hand).maxThree), Ranking = (int)Ranking.FULL_HOUSE };
            if (IsFlush(hand).check) return new CheckRanking { MaxValue = IsFlush(hand).max, Kickers = GetKickers(cards, IsFlush(hand).max), Ranking = (int)Ranking.FLUSH };
            if (IsStraight(hand).check) return new CheckRanking { MaxValue = IsStraight(hand).max, Kickers = [], Ranking = (int)Ranking.STRAIGHT };
            if (IsThreeOfAKind(hand).check) return new CheckRanking { MaxValue = IsThreeOfAKind(hand).max, Kickers = GetKickers(cards, IsThreeOfAKind(hand).max), Ranking = (int)Ranking.THREE_OF_A_KIND };
            if (IsTwoPair(hand).check) return new CheckRanking { MaxValue = IsTwoPair(hand).max, Kickers = GetKickers(cards, IsTwoPair(hand).max), Ranking = (int)Ranking.TWO_PAIR };
            if (IsPair(hand).check) return new CheckRanking { MaxValue = IsPair(hand).max, Kickers = GetKickers(cards, IsPair(hand).max), Ranking = (int)Ranking.PAIR };
            return new CheckRanking { MaxValue = (int)cards[0], Kickers = cards.Skip(1).ToArray(), Ranking = (int)Ranking.HIGH_CARD };
        }
        private static int[] GetKickers(int[] allCards, int mainHandValue)
        {
            int[] kickers = allCards.Where(v => v != mainHandValue).OrderByDescending(v => v).ToArray();
            return kickers;
        }

        private static bool IsRoyalFlush(Player hand)
        {
            (bool check, int max) straightFlush = IsStraightFlush(hand);
            return straightFlush.check && hand.Cards[0].Rank == Enums.Rank.Ace;
        }

        private static (bool check, int max) IsStraightFlush(Player hand)
        {
            (bool check, int max) straight = IsStraight(hand);
            (bool check, int max) flush = IsFlush(hand);
            int max = Math.Max(straight.max, flush.max);
            return (straight.check && straight.check, max);
        }

        private static (bool check, int max) IsFourOfAKind(Player hand)
        {
            return (hand.Cards.GroupBy(c => c.Rank).Any(g => g.Count() == 4), hand.Cards.Select(c => (int)c.Rank).OrderBy(r => r).ToList().Max());
        }

        private static (bool check, int maxThree) IsFullHouse(Player hand)
        {
            var groups = hand.Cards.GroupBy(c => c.Rank)
                            .Select(g => new { Rank = g.Key, Count = g.Count() })
                            .ToList();

            int maxThree = groups.Where(g => g.Count == 3)
                                 .Select(g => (int)g.Rank)
                                 .DefaultIfEmpty(0)
                                 .Max();

            int maxTwo = groups.Where(g => g.Count == 2)
                               .Select(g => (int)g.Rank)
                               .DefaultIfEmpty(0)
                               .Max();

            bool isFullHouse = maxThree > 0 && maxTwo > 0;

            return (isFullHouse, maxThree);
        }


        private static (bool check, int max) IsFlush(Player hand)
        {
            bool isFlush = hand.Cards.Select(c => c.Suit).Distinct().Count() == 1;

            int maxCardValue = isFlush ? hand.Cards.Max(c => (int)c.Rank) : 0;

            return (isFlush, maxCardValue);
        }

        private static (bool check, int max) IsStraight(Player hand)
        {
            List<int> ranks = hand.Cards.Select(c => (int)c.Rank).OrderBy(r => r).ToList();

            List<int> uniqueRanks = ranks.Distinct().ToList();

            if (uniqueRanks.Count < 5) return (false, 0);

            for (int i = 0; i <= uniqueRanks.Count - 5; i++)
            {
                if (uniqueRanks[i + 4] - uniqueRanks[i] == 4)
                    return (true, uniqueRanks[i + 4]);
            }
            if (uniqueRanks.Contains(14) && uniqueRanks.Contains(2) && uniqueRanks.Contains(3) && uniqueRanks.Contains(4) && uniqueRanks.Contains(5))
                return (true, 5);

            return (ranks.Max() - ranks.Min() == 4, ranks.Max());
        }

        private static (bool check, int max) IsThreeOfAKind(Player hand)
        {
            var groups = hand.Cards.GroupBy(c => c.Rank)
                                   .Where(g => g.Count() == 3)
                                   .ToList();

            bool isThreeOfAKind = groups.Any();
            int maxRank = isThreeOfAKind ? groups.Max(g => (int)g.Key) : 0;

            return (isThreeOfAKind, maxRank);
        }

        private static (bool check, int max) IsTwoPair(Player hand)
        {
            var pairs = hand.Cards.GroupBy(c => c.Rank)
                                  .Where(g => g.Count() == 2)
                                  .ToList();

            bool isTwoPair = pairs.Count == 2;
            int maxRank = isTwoPair ? pairs.Max(g => (int)g.Key) : 0;

            return (isTwoPair, maxRank);
        }
        private static (bool check, int max) IsPair(Player hand)
        {
            var pairs = hand.Cards.GroupBy(c => c.Rank)
                                  .Where(g => g.Count() == 2)
                                  .ToList();

            bool isPair = pairs.Any();
            int maxRank = isPair ? pairs.Max(g => (int)g.Key) : 0;

            return (isPair, maxRank);
        }

    }
}