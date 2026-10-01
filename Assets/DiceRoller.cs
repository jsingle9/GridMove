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

    /// <summary>
    /// Roll dice with optional modifier.
    /// Formats: "XdY", "XdY+Z", "XdY-Z"
    /// Examples: "1d20", "2d6+3", "3d4-1"
    /// </summary>
    public static int Roll(string dice)
    {
        dice = dice.ToLower().Trim();

        int modifier = 0;

        // Parse modifier if present
        if (dice.Contains('+'))
        {
            string[] split = dice.Split('+');
            dice = split[0].Trim();
            modifier = int.Parse(split[1].Trim());
        }
        else if (dice.Contains('-') && dice.IndexOf('-') > 0) // Ignore leading minus
        {
            int lastDashIndex = dice.LastIndexOf('-');
            string modifierPart = dice.Substring(lastDashIndex + 1);
            dice = dice.Substring(0, lastDashIndex).Trim();

            if (int.TryParse(modifierPart, out int mod))
                modifier = -mod;
        }

        // Parse dice notation
        string[] parts = dice.Split('d');

        if (parts.Length != 2)
        {
            Debug.LogError($"Invalid dice format: {dice}");
            return 0;
        }

        int num = int.Parse(parts[0].Trim());
        int sides = int.Parse(parts[1].Trim());

        int total = 0;

        for (int i = 0; i < num; i++)
            total += Random.Range(1, sides + 1);

        return total + modifier;
    }
}
