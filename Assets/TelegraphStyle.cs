using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Telegraph Style", fileName = "TelegraphStyle")]
public class TelegraphStyle : ScriptableObject
{
    [Header("Prefab")]
    public GameObject tilePrefab;   // fire tile prefab

    [Header("Tint/Scale/Timing")]
    public Color color = new(1f, 0.3f, 0.1f, 0.5f);
    public Vector3 scale = Vector3.one;
    public float duration = 1.8f;
}
