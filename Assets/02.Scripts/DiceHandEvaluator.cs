using System.Linq;

public enum DiceHandType
{
    HighCard,     // 족보 없음
    OnePair,      // 2개 동일
    TwoPair,      // 2개 동일 + 다른 2개 동일
    ThreeOfAKind, // 3개 동일
    Straight,     // 12345 또는 23456
    FullHouse,    // 3개 동일 + 2개 동일
    FourOfAKind,  // 4개 동일
    FiveOfAKind   // 5개 동일
}

public struct DiceHandResult
{
    public DiceHandType HandType;     // 타워의 종류/외형 결정
    public int PrimaryValue;          // 데미지 배율 결정
}

public static class DiceHandEvaluator
{
    public static DiceHandResult Evaluate(int[] dice)
    {
        if (dice == null || dice.Length != 5) 
            return new DiceHandResult { HandType = DiceHandType.HighCard, PrimaryValue = 0 };

        // 개수 순으로 내림차순, 개수가 같으면 눈금 순으로 내림차순 정렬
        var groupedDice = dice.GroupBy(v => v)
                              .Select(g => new { Value = g.Key, Count = g.Count() })
                              .OrderByDescending(g => g.Count)
                              .ThenByDescending(g => g.Value)
                              .ToList();

        int primary = groupedDice[0].Value;
        DiceHandType type = DiceHandType.HighCard;

        if (groupedDice[0].Count == 5) 
            type = DiceHandType.FiveOfAKind;
        else if (groupedDice[0].Count == 4) 
            type = DiceHandType.FourOfAKind;
        else if (groupedDice[0].Count == 3 && groupedDice[1].Count == 2) 
            type = DiceHandType.FullHouse;
        else if (groupedDice[0].Count == 3) 
            type = DiceHandType.ThreeOfAKind;
        else if (groupedDice[0].Count == 2 && groupedDice[1].Count == 2) 
            type = DiceHandType.TwoPair;
        else if (groupedDice[0].Count == 2) 
            type = DiceHandType.OnePair;
        else
        {
            int max = dice.Max();
            int min = dice.Min();
            
            if (max - min == 4) 
            {
                type = DiceHandType.Straight;
                primary = max;
            }
        }

        return new DiceHandResult 
        { 
            HandType = type, 
            PrimaryValue = primary 
        };
    }
}