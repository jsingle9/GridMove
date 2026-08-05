using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Telegraph Style")]
public class TelegraphStyle : ScriptableObject
{
    public Color color = new(1f, 0.3f, 0.1f, 0.5f);
    public Vector3 scale = Vector3.one;
    public float duration = 0.8f;
}
