using System.Collections.Generic;
using UnityEngine;

public class BreathWeaponTelegrapher : MonoBehaviour
{
    [Header("Telegraph Visuals")]
    [SerializeField] private GameObject warningTilePrefab;
    [SerializeField] private Transform telegraphRoot;
    [SerializeField] private bool rotateToMatchDirection = false;

    private readonly List<GameObject> spawnedWarnings = new List<GameObject>();

    void Awake()
    {
        if (telegraphRoot == null)
            telegraphRoot = transform;

        Debug.Log($"[Telegrapher] Awake on {gameObject.name}. Prefab assigned: {warningTilePrefab != null}");
    }

    public void ShowTelegraph(GridController grid, IEnumerable<Vector3Int> cells, Vector3Int direction)
    {
        ClearTelegraph();

        Debug.Log($"[Telegrapher] ShowTelegraph called. Prefab assigned: {warningTilePrefab != null}, root: {telegraphRoot.name}");

        if (warningTilePrefab == null)
        {
            Debug.LogError("[Telegrapher] warningTilePrefab is NULL.");
            return;
        }

        Quaternion rotation = Quaternion.identity;

        if (rotateToMatchDirection)
        {
            if (direction == Vector3Int.right)
                rotation = Quaternion.identity;
            else if (direction == Vector3Int.left)
                rotation = Quaternion.Euler(0f, 0f, 180f);
            else if (direction == Vector3Int.up)
                rotation = Quaternion.Euler(0f, 0f, 90f);
            else if (direction == Vector3Int.down)
                rotation = Quaternion.Euler(0f, 0f, -90f);
        }

        int count = 0;

        foreach (Vector3Int cell in cells)
        {
            Vector3 worldPos = grid.GridToWorld(cell);
            Debug.Log($"[Telegrapher] Spawning warning at cell {cell}, world {worldPos}");

            GameObject warning = Instantiate(warningTilePrefab, worldPos, rotation, telegraphRoot);
            warning.name = $"BreathWarning_{cell.x}_{cell.y}";

            warning.transform.localScale = Vector3.one;

            SpriteRenderer sr = warning.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(1f, 0.25f, 0f, 0.85f);
                sr.sortingOrder = 100;
                Debug.Log($"[Telegrapher] SpriteRenderer found on {warning.name}");
            }
            else
            {
                Debug.LogWarning($"[Telegrapher] No SpriteRenderer found on {warning.name}");
            }

            spawnedWarnings.Add(warning);
            count++;
        }

        Debug.Log($"[Telegrapher] Spawned {count} warning tiles");
    }

    public void ClearTelegraph()
    {
        for (int i = 0; i < spawnedWarnings.Count; i++)
        {
            if (spawnedWarnings[i] != null)
                Destroy(spawnedWarnings[i]);
        }

        spawnedWarnings.Clear();
    }
}
