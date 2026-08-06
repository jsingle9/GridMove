using System.Collections.Generic;
using UnityEngine;

public class SpellTelegrapher : MonoBehaviour
{
    [SerializeField] private GameObject defaultTilePrefab;
    [SerializeField] private Transform root;

    private readonly List<GameObject> spawned = new();

    public void Show(GridController grid, List<Vector3Int> cells, TelegraphStyle style)
    {
        Clear();
        if (grid == null || cells == null || style == null) return;

        GameObject prefab = style.tilePrefab != null ? style.tilePrefab : defaultTilePrefab;
        if (prefab == null) return;

        foreach (var cell in cells)
        {
            if (!grid.IsInBounds(cell)) continue;
            var world = grid.GridToWorld(cell);

            var go = Instantiate(prefab, world, Quaternion.identity, root ? root : transform);
            go.transform.localScale = style.scale;

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = style.color;

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
