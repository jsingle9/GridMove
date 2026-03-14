using UnityEngine;

public class TileVisual : MonoBehaviour
{
    SpriteRenderer sr;
    Color baseColor;
    public bool isHighlighted;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetBaseColor(Color color)
    {
        baseColor = color;

        if(!isHighlighted)
          sr.color = color;

    }

    public void Highlight()
    {
        Debug.Log("Highlight called on " + gameObject.name);
        Debug.Log("Highlighting tile " + transform.position);
        isHighlighted = true;
        sr.color = Color.yellow;

    }

    public void ClearHighlight()
    {
        Debug.Log("TileVisual.ClearHighlight() Called");
        isHighlighted = false;
        sr.color = baseColor;
    }
}
