using UnityEngine;

public class MoveIntent : Intent
{
    public Vector3Int targetCell;

    public MoveIntent(Vector3Int target)
    {
        targetCell = target;
    }
}
