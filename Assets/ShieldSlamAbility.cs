using UnityEngine;

/// <summary>
/// Fighter save-based attack:
/// - Target makes STR save vs DC
/// - Fail: push 2 tiles
/// - Success: push 1 tile
/// - Optional small damage on fail
/// </summary>
public class ShieldSlamAbility : Ability
{
    private const int BaseDc = 13; // simple test DC; can make dynamic later

    public override string Name => "Shield Slam";
    public override bool RequiresTarget => true;
    public override int Range => 1; // adjacent

    public override AbilityResult Execute(ICombatant user, TargetData target)
    {
        if (user == null || target == null || target.PrimaryTarget == null)
            return AbilityResult.Failure("Invalid target.");

        ICombatant defender = target.PrimaryTarget;

        // Must be adjacent
        Vector3Int userCell = GridPosition(user);
        Vector3Int defenderCell = GridPosition(defender);
        int manhattan = Mathf.Abs(userCell.x - defenderCell.x) + Mathf.Abs(userCell.y - defenderCell.y);
        if (manhattan > 1)
            return AbilityResult.Failure("Target is not adjacent.");

        // Saving throw
        int saveRoll = SavingThrowUtility.RollSave(defender, AbilityScoreType.STR);
        bool success = saveRoll >= BaseDc;

        int pushTiles = success ? 1 : 2;
        bool pushed = KnockbackUtility.TryPush(defender, userCell, defenderCell, pushTiles);

        // Optional fail damage (small, for feedback)
        if (!success)
        {
            int damage = Random.Range(1, 7); // 1d6
            defender.TakeDamage(damage);
            Debug.Log($"{Name}: {defender.Name} FAILS STR save ({saveRoll} vs DC {BaseDc}), takes {damage}, pushed {pushTiles}.");
        }
        else
        {
            Debug.Log($"{Name}: {defender.Name} SUCCEEDS STR save ({saveRoll} vs DC {BaseDc}), pushed {pushTiles}.");
        }

        if (!pushed)
            Debug.Log($"{Name}: no valid tile to push into.");

        return AbilityResult.SuccessResult();
    }

    private Vector3Int GridPosition(ICombatant c)
    {
        return GridRegistry.Grid.WorldToGrid(c.GetWorldPosition());
    }
}
