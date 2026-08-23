using UnityEngine;

public enum RollMode
{
    Normal,
    Advantage,
    Disadvantage
}

public struct D20RollResult
{
    public int FirstDie;
    public int SecondDie;   // 0 when normal
    public int KeptDie;
    public RollMode Mode;
}

public static class DiceRoller
{
    public static int RollD20()
    {
        return Random.Range(1, 21);
    }

    public static D20RollResult RollD20(RollMode mode)
    {
        int a = Random.Range(1, 21);

        if (mode == RollMode.Normal)
        {
            return new D20RollResult
            {
                FirstDie = a,
                SecondDie = 0,
                KeptDie = a,
                Mode = RollMode.Normal
            };
        }

        int b = Random.Range(1, 21);
        int kept = (mode == RollMode.Advantage) ? Mathf.Max(a, b) : Mathf.Min(a, b);

        return new D20RollResult
        {
            FirstDie = a,
            SecondDie = b,
            KeptDie = kept,
            Mode = mode
        };
    }

    public static int Roll(string dice)
    {
        // expects format "XdY"
        string[] parts = dice.ToLower().Split('d');

        int num = int.Parse(parts[0]);
        int sides = int.Parse(parts[1]);

        int total = 0;

        for (int i = 0; i < num; i++)
            total += Random.Range(1, sides + 1);

        return total;
    }
}
