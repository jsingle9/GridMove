using UnityEngine;

public class GridNode{

  public Vector3Int gridPos;
  public bool walkable;

  public int gCost;
  public int hCost;
  public int fCost => gCost + hCost;

  public GridNode parent;

  public GridNode(Vector3Int pos, bool walkable){
    this.gridPos = pos;
    this.walkable = walkable;
  }

}
