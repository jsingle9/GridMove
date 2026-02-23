using UnityEngine;

public class AttackIntent : Intent
{
    public ICombatant target;

    public AttackIntent(ICombatant enemy)
    {
        target = enemy;
    }
}
