
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObstacleTilemapScanner : MonoBehaviour
{
    [SerializeField] Tilemap obstacleTilemap;
    [SerializeField] GridController gridController;

    void Start()
    {
        if (obstacleTilemap == null || gridController == null)
        {
            Debug.LogError("ObstacleTilemapScanner not configured", this);
            return;
        }

        foreach (Vector3Int cell in obstacleTilemap.cellBounds.allPositionsWithin)
        {
            if (!obstacleTilemap.HasTile(cell))
                continue;

            gridController.SetWalkable(cell, false);
        }
    }
}
