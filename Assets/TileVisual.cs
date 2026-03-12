using UnityEngine;

public class TileVisual : MonoBehaviour
{
    SpriteRenderer sr;
    Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    public void Highlight()
    {
        sr.color = Color.yellow;
    }

    public void ClearHighlight()
    {
        sr.color = baseColor;
    }
}
