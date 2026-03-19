using System.Collections.Generic;

public class AbilityResult
{
    public bool Success { get; set; }
    public string FailureReason { get; set; }

    public AbilityResult()
    {
        Success = false;
        FailureReason = "";
    }

    public static AbilityResult CreateSuccess()
    {
        return new AbilityResult { Success = true };
    }

    public static AbilityResult CreateFailure(string reason)
    {
        return new AbilityResult
        {
            Success = false,
            FailureReason = reason
        };
    }
}
