using System.Collections.Generic;
using UnityEngine;

public class SpellTelegrapher : MonoBehaviour
{
    [SerializeField] private GameObject defaultTilePrefab;
    [SerializeField] private Transform root;

    private readonly List<GameObject> spawned = new();

    public void Show(GridController grid, List<Vector3Int> cells, Color color, Vector3 scale)
    {
        Clear();
        if (grid == null || defaultTilePrefab == null || cells == null) return;

        foreach (var cell in cells)
        {
            if (!grid.IsInBounds(cell)) continue;

            var world = grid.GridToWorld(cell);
            var go = Instantiate(defaultTilePrefab, world, Quaternion.identity, root ? root : transform);
            go.transform.localScale = scale;

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = color;

            spawned.Add(go);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null) Destroy(spawned[i]);
        spawned.Clear();
    }
}
