using UnityEngine;

public class TileVisual : MonoBehaviour
{
    SpriteRenderer sr;
    Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetBaseColor(Color color)
    {
        baseColor = color;
        sr.color = color;
    }

    public void Highlight()
    {
        Debug.Log("Highlight called on " + gameObject.name);
        Debug.Log("Highlighting tile " + transform.position);
        sr.color = Color.yellow;
    }

    public void ClearHighlight()
    {
        sr.color = baseColor;
    }
}
