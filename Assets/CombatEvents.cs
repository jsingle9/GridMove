using System;

public static class CombatEvents
{
    public static Action<string> OnLog;

    public static void Log(string message)
    {
        OnLog?.Invoke(message);
    }
}
