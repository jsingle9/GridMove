using System.Collections.Generic;
using UnityEngine;

public class SpellTelegrapher : MonoBehaviour
{
    [SerializeField] private GameObject defaultTilePrefab;
    [SerializeField] private Transform root;

    private readonly List<GameObject> spawned = new();

  /*  public void Show(GridController grid, List<Vector3Int> cells, TelegraphStyle style)
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

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Highlight";
                sr.sortingOrder = 10;
                sr.color = style.color;
            }

            spawned.Add(go);
        }
    } */

    public void Show(GridController grid, List<Vector3Int> cells, TelegraphStyle style)
    {
        Debug.Log("[Telegraph] Show called");
        Clear();
        if (grid == null) { Debug.Log("[Telegraph] grid null"); return; }
        if (cells == null) { Debug.Log("[Telegraph] cells null"); return; }
        if (style == null) { Debug.Log("[Telegraph] style null"); return; }

        GameObject prefab = style.tilePrefab != null ? style.tilePrefab : defaultTilePrefab;
        if (prefab == null) { Debug.Log("[Telegraph] prefab null"); return; }

        Debug.Log($"[Telegraph] Spawning {cells.Count} tiles from prefab {prefab.name}");

        foreach (var cell in cells)
        {
            if (!grid.IsInBounds(cell)) continue;

            var world = grid.GridToWorld(cell);
            var go = Instantiate(prefab, world, Quaternion.identity, root ? root : transform);

            go.transform.localScale = style.scale;
            go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -0.1f);

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Highlight";
                sr.sortingOrder = 100;
                sr.color = new Color(1f, 1f, 1f, 1f); // force visible for test
                Debug.Log($"[Telegraph] Renderer ok on {go.name}, layer={sr.sortingLayerName}, order={sr.sortingOrder}");
            }
            else
            {
                Debug.LogWarning($"[Telegraph] No SpriteRenderer found on {go.name}");
            }

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
