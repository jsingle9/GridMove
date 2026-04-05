using UnityEngine;
using System.Collections.Generic;

public class MiniBossEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();

        // Override to mini boss-specific stats
        maxHP = 25;
        currentHP = maxHP;
        equippedWeapon = new Weapon("Legendary Blade", 4, "1d10");
        armorClass = 14;
        attackBonus = 5;
        damageDice = "1d8";
        damageModifier = 3;
        speed = 6;

        // Mini boss has multiple abilities
        abilities.Clear();
        abilities.Add(new AttackAbility());
        abilities.Add(new RangedAttackAbility());
        abilities.Add(new FireballAbility());

        Debug.Log("MiniBossEnemy initialized");
    }

    protected override System.Collections.IEnumerator EnemyTurnRoutine()
    {
        if(IsDead()) yield break;

        yield return new WaitForSeconds(0.15f);

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if(player == null)
        {
            CombatManager.Instance.EndTurn();
            yield break;
        }

        float distToPlayer = Vector3.Distance(GetWorldPosition(), player.GetWorldPosition());

        // AI: Use Fireball if > 3 units away and lots of HP
        if(distToPlayer > 3f && currentHP > maxHP / 2)
        {
            if(HasAction)
            {
                Vector3 playerPos = player.GetWorldPosition();
                GridNode playerNode = grid.GetNodeFromWorld(playerPos);
                TargetData fireballTarget = new TargetData(player);
                fireballTarget.tile = playerNode;

                Ability fireball = abilities[2];
                intentExecutor.ExecuteAbilityWithMovement(this, fireball, fireballTarget);

                while(mover.IsMoving)
                    yield return null;

                yield return new WaitForSeconds(0.1f);
            }
        }
        // Otherwise use melee if close
        else if(distToPlayer < 2f && HasAction)
        {
            Ability melee = abilities[0];
            TargetData targetData = new TargetData(player);
            intentExecutor.ExecuteAbilityWithMovement(this, melee, targetData);

            while(mover.IsMoving)
                yield return null;

            yield return new WaitForSeconds(0.1f);
        }
        // Move closer if too far
        else if(HasMove)
        {
            GridNode startNode = grid.GetNodeFromWorld(transform.position);
            List<GridNode> path = resolver.Resolve(new AttackIntent(new TargetData(player)), startNode);

            if(path != null && path.Count > 0)
            {
                int moveCost = path.Count - 1;
                if(moveCost > RemainingMovement)
                    path = path.GetRange(0, RemainingMovement + 1);

                mover.StartPath(path);
                RemainingMovement -= (path.Count - 1);
                HasMove = RemainingMovement > 0;

                while(mover.IsMoving)
                    yield return null;
            }
        }

        yield return new WaitForSeconds(0.1f);
        EndMyTurn();
    }
    
    protected override void Die()
    {
        Debug.Log($"{name} died");
        Vector3Int deathCell = grid.WorldToGrid(transform.position);
        grid.UnregisterOccupant(deathCell);

        // Mini boss drops 2H sword
        GameObject weaponDropObj = new GameObject("Loot_TwoHandedSword");
        weaponDropObj.transform.position = transform.position;
        LootDrop weaponLoot = weaponDropObj.AddComponent<LootDrop>();
        Weapon twoHandedSword = new Weapon("Two Handed Sword", 3, "1d12");
        weaponLoot.SetWeapon(twoHandedSword);

        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }
}
