using UnityEngine;
using System.Collections.Generic;

public class MeleeEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();

        // Melee-specific initialization
        equippedWeapon = new Weapon("Short Sword", 0, "1d6");
        abilities.Add(new AttackAbility());

        Debug.Log("MeleeEnemy abilities: " + abilities.Count);
    }

    protected override System.Collections.IEnumerator EnemyTurnRoutine()
    {
        if(IsDead()) yield break;

        yield return new WaitForSeconds(0.15f);

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if(player == null)
        {
            EndMyTurn();
            yield break;
        }

        // Try attack first
        Ability chosen = ChooseAbility(player);
        TargetData targetData = new TargetData(player);

        AbilityResult result = intentExecutor.ExecuteAbilityWithMovement(this, chosen, targetData);

        // If we moved, wait until movement finishes
        while(mover.IsMoving)
        {
            yield return null;
        }

        // Wait a bit for pending ability execution
        yield return new WaitForSeconds(0.1f);

        // If still have action, try attack again
        if(HasAction)
        {
            chosen = ChooseAbility(player);
            targetData = new TargetData(player);
            result = intentExecutor.ExecuteAbilityWithMovement(this, chosen, targetData);
        }

        // Wait for any final movement
        while(mover.IsMoving)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        Debug.Log("MeleeEnemy finished turn");
        EndMyTurn();
    }

    Ability ChooseAbility(ICombatant target)
    {
        float dist = Vector3.Distance(
            GetWorldPosition(),
            target.GetWorldPosition()
        );

        Ability melee = null;
        Ability ranged = null;

        foreach(var a in abilities)
        {
            if(a is RangedAttackAbility) ranged = a;
            if(a is AttackAbility) melee = a;
        }

        // PRIORITY 1: If ranged exists and usable → use it
        if(ranged != null && ranged.CanUse(this))
        {
            Debug.Log("Choosing ranged");
            return ranged;
        }

        // PRIORITY 2: fallback melee
        if(melee != null && melee.CanUse(this))
        {
            Debug.Log("Choosing melee fallback");
            return melee;
        }

        Debug.Log("No valid ability");
        return melee;
    }

    protected override void Die()
    {
        Debug.Log($"{name} died");
        Vector3Int deathCell = grid.WorldToGrid(transform.position);
        grid.UnregisterOccupant(deathCell);

        // Melee enemies drop healing potions
        GameObject potionDropObj = new GameObject("Loot_HealingPotion");
        potionDropObj.transform.position = transform.position;
        LootDrop potionLoot = potionDropObj.AddComponent<LootDrop>();
        HealingPotion potion = ScriptableObject.CreateInstance<HealingPotion>();
        potionLoot.SetPotion(potion);

        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }
        
}
