
public interface ICombatant{
    int Initiative { get; set; }
    void StartTurn();
    void EndTurn();

    bool HasMove { get; set; }
    bool HasAction { get; set; }
}
