public abstract class StatusEffect
{
    public string Name { get; private set; }
    public int RemainingTurns { get; private set; }

    public StatusEffect(string name, int duration)
    {
        Name = name;
        RemainingTurns = duration;
    }

    public virtual void OnApply(ICombatant target) { }
    public virtual void OnTurnStart(ICombatant target) { }
    public virtual void OnTurnEnd(ICombatant target) { }
    public virtual void OnExpire(ICombatant target) { }

    public void Tick(ICombatant target)
    {
        RemainingTurns--;

        if (RemainingTurns <= 0)
        {
            OnExpire(target);
            target.RemoveStatus(this);
        }
    }
}
