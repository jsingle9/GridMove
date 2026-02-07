using UnityEngine;
using UnityEngine.InputSystem;

public class HighlightObject : MonoBehaviour
{
  public GameObject highlight;   // The highlight sprite/object
  public Grid grid;              // Reference to your Grid
  private Vector3Int lastCell = new Vector3Int(-1, -1, 0);

  void Update()
  {
      Vector3 mousePos = Camera.main.ScreenToWorldPoint(
          Mouse.current.position.ReadValue()
      );
      mousePos.z = 0;

      Vector3Int cell = grid.WorldToCell(mousePos);

      if (cell != lastCell)
      {
          HighlightCell(cell);
          lastCell = cell;
      }
  }

  void HighlightCell(Vector3Int cell)
  {
      Vector3 worldPos = grid.GetCellCenterWorld(cell);
      highlight.transform.position = worldPos;
  }
}
