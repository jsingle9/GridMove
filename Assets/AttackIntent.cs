using UnityEngine;

public class AttackIntent : Intent
{
    public Enemy target;

    public AttackIntent(Enemy enemy)
    {
        target = enemy;
    }
}
