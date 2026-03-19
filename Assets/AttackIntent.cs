using UnityEngine;

public class AttackIntent : Intent
{
    public TargetData data;

    public AttackIntent(TargetData tData)
    {
        this.data = tData;
    }
}
