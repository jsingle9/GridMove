using UnityEngine;

public static class DiceRoller
{
    public static int RollD20()
    {
        return Random.Range(1, 21);
    }

    public static int Roll(string dice)
    {
        // expects format "XdY"
        string[] parts = dice.ToLower().Split('d');

        int num = int.Parse(parts[0]);
        int sides = int.Parse(parts[1]);

        int total = 0;

        for(int i = 0; i < num; i++)
            total += Random.Range(1, sides + 1);

        return total;
    }
}
