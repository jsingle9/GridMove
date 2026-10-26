using UnityEngine;

public class TileVisual : MonoBehaviour
{
    SpriteRenderer sr;
    Color baseColor;
    public bool isHighlighted;

    [SerializeField] private string sortingLayerName = "Highlight";
    [SerializeField] private int baseSortingOrder = 0;
    [SerializeField] private int highlightSortingOrder = 2;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        //Debug.Log($"[TileVisual.Awake] {gameObject.name}: sr={sr?.gameObject.name ?? "null"}");

        if (sr != null)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = baseSortingOrder;
          //  Debug.Log($"[TileVisual.Awake] Set sorting layer to '{sortingLayerName}' order {baseSortingOrder}");
        }
    }

    public void SetBaseColor(Color color)
    {
        baseColor = color;

        if(!isHighlighted)
          sr.color = color;

    }

    public void Highlight()
    {
        Debug.Log($"[TileVisual.Highlight] Called on {gameObject.name}");
        isHighlighted = true;

        if (sr == null)
        {
            Debug.LogError($"[TileVisual.Highlight] SpriteRenderer is null on {gameObject.name}!");
            return;
        }

        Debug.Log($"[TileVisual.Highlight] Before: sr.color = {sr.color}");
        sr.color = new Color(1f, 1f, 0f, 1f); // Yellow, full opacity
        Debug.Log($"[TileVisual.Highlight] After: sr.color = {sr.color}");
    }

    public void ClearHighlight()
    {
        //Debug.Log($"[TileVisual.ClearHighlight] Called on {gameObject.name}");
        isHighlighted = false;

        /*if (sr == null)
        {
            Debug.LogError($"[TileVisual.ClearHighlight] SpriteRenderer is null!");
            return;
        }*/

        //Debug.Log($"[TileVisual.ClearHighlight] Setting color to {baseColor}");
        sr.color = baseColor;
    }
}
