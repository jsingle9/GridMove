using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTelegraphSystem : MonoBehaviour
{
    [SerializeField] private GameObject defaultDamageTilePrefab;
    [SerializeField] private Transform root;

    private readonly List<GameObject> spawned = new();

    /// <summary>
    /// Shows a damage telegraph on a target that fades out after duration.
    /// </summary>
    public void ShowDamageTelegraph(ICombatant target, int damage, TelegraphStyle style, float duration = 1f)
    {
        if (target == null)
        {
            Debug.LogWarning("[DamageTelegraph] Target is null");
            return;
        }

        GameObject prefab = style?.tilePrefab != null ? style.tilePrefab : defaultDamageTilePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[DamageTelegraph] No prefab available");
            return;
        }

        Vector3 targetWorldPos = target.GetWorldPosition();
        GameObject go = Instantiate(
            prefab,
            targetWorldPos,
            Quaternion.identity,
            root ? root : transform
        );

        go.transform.localScale = style?.scale ?? Vector3.one;
        go.transform.position = new Vector3(
            go.transform.position.x,
            go.transform.position.y,
            -0.05f
        );

        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Highlight";
            sr.sortingOrder = 200;
            sr.color = style?.color ?? new Color(1f, 0.3f, 0.3f, 1f); // Default: red
        }

        spawned.Add(go);
        StartCoroutine(FadeOutAndRemove(go, duration));
    }

    private IEnumerator FadeOutAndRemove(GameObject go, float duration)
    {
        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            yield return new WaitForSeconds(duration);
            RemoveAndDestroy(go);
            yield break;
        }

        Color originalColor = sr.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            Color newColor = originalColor;
            newColor.a = alpha;
            sr.color = newColor;
            yield return null;
        }

        RemoveAndDestroy(go);
    }

    private void RemoveAndDestroy(GameObject go)
    {
        if (go != null)
        {
            spawned.Remove(go);
            Destroy(go);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null) Destroy(spawned[i]);
        spawned.Clear();
    }
}
