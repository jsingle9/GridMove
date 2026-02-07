using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections.Generic;


public class GridController : MonoBehaviour
{

     public GridNode[,] grid;
    [SerializeField] int width = 256;   // -5 to +5
    [SerializeField] int height = 256;
    [SerializeField] Vector3Int gridOrigin = new Vector3Int(-128, -128, 0);
    public Tilemap obstacleTilemap;

    void Awake(){

      grid = new GridNode[width, height];

      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
            Vector3Int cellPos = new Vector3Int(
                x + gridOrigin.x,
                y + gridOrigin.y,
                0
              );

            grid[x, y] = new GridNode(cellPos, true);
        }
      }
    }
    // constructor
    void Start(){


    }

    void Update(){



    }

    public GridNode GetNodeFromWorld(Vector3 worldPos)
    {
      Vector3Int cell = WorldToGrid(worldPos);

      if (!InBounds(cell)) return null;

      int x = cell.x - gridOrigin.x;
      int y = cell.y - gridOrigin.y;

      return grid[x, y];
    }

    // convert world position to gridPosition
    public Vector3Int WorldToGrid(Vector3 worldPos){
      return new Vector3Int(Mathf.FloorToInt(worldPos.x),
      Mathf.FloorToInt(worldPos.y), 0);
    }

    // convert grid position to world gridPosition
    public Vector3 GridToWorld(Vector3Int gridPos){
      return new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0);
      // Adjust to center of center of the grid cell
    }

    private bool InBounds(Vector3Int cell){
      int x = cell.x - gridOrigin.x;
      int y = cell.y - gridOrigin.y;

      return x >= 0 && y >= 0 && x < width && y < height;
    }

    public bool IsWalkable(Vector3Int cell){
      if (!InBounds(cell)) return false;

      int x = cell.x - gridOrigin.x;
      int y = cell.y - gridOrigin.y;

      return grid[x, y].walkable;
    }

    public void SetWalkable(Vector3Int cell, bool walkable){
      if (!InBounds(cell)) return;

      int x = cell.x - gridOrigin.x;
      int y = cell.y - gridOrigin.y;

      grid[x, y].walkable = walkable;
    }

    public List<GridNode> GetNeighbors(GridNode node)
    {
      List<GridNode> neighbors = new List<GridNode>();

      Vector3Int[] directions =
      {
          Vector3Int.up,
          Vector3Int.down,
          Vector3Int.left,
          Vector3Int.right
      };

      foreach (var dir in directions)
      {
          Vector3Int checkPos = node.gridPos + dir;

          if (!InBounds(checkPos)) continue;

          int x = checkPos.x - gridOrigin.x;
          int y = checkPos.y - gridOrigin.y;

          neighbors.Add(grid[x, y]);
      }

    return neighbors;
}

    void OnDrawGizmos(){
      Gizmos.color = Color.red;

      // start start main grid drawing WorldToViewportPoint
      for(int x = -128; x <= 128; x++){ // These bounds control grid size in x
          for(int y = -128; y <= 128; y++){ // Bounds control size in y direction
            Gizmos.DrawWireCube(new Vector3(x + 0.5f, y + 0.5f, 0),
                new Vector3(1f, 1f, 0)); //Each cell size is 1
          }
      }
    }

}
