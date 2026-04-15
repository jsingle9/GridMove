using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections.Generic;


public class GridController : MonoBehaviour
{

    Dictionary<Vector3Int, ICombatant> occupiedTiles =
    new Dictionary<Vector3Int, ICombatant>();
    public GameObject tilePrefab;
    TileVisual[,] tileVisuals;

    public GridNode[,] grid;
    [SerializeField] int width = 256;   // -5 to +5
    [SerializeField] int height = 256;
    [SerializeField] Vector3Int gridOrigin = new Vector3Int(-128, -128, 0);
    public Tilemap obstacleTilemap;

    void Awake(){

      grid = new GridNode[width, height];
      tileVisuals = new TileVisual[width, height];

      for (int x = 0; x < width; x++){

        for (int y = 0; y < height; y++){

            Vector3Int cellPos = new Vector3Int(
                x + gridOrigin.x,
                y + gridOrigin.y,
                0
            );
            grid[x, y] = new GridNode(cellPos, true);

            // Visual tile overlay
            Vector3 visualPos = new Vector3(cellPos.x + 0.5f,
              cellPos.y + 0.5f, 0
            );
            GameObject tileObj = Instantiate(tilePrefab, visualPos,
              Quaternion.identity, transform
            );
            TileVisual visual = tileObj.GetComponent<TileVisual>();

            tileVisuals[x, y] = visual;
            Color tileColor = ((x + y) % 2 == 0)
            ? new Color(0.35f, 0.65f, 0.35f)
            : new Color(0.3f, 0.6f, 0.3f);
            visual.SetBaseColor(tileColor);
        }
      }
    }

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

      if(occupiedTiles.ContainsKey(cell))
          return false;

      int x = cell.x - gridOrigin.x;
      int y = cell.y - gridOrigin.y;

      return grid[x, y].walkable;
    }

    public void SetWalkable(Vector3Int cell, bool walkable){

      Debug.Log($"SetWalkable called: cell={cell}, walkable={walkable}");
      //if(!InBounds(cell)) return;

      if(!InBounds(cell))
      {
          Debug.LogError($"SetWalkable OUT OF BOUNDS: {cell}");
          return;
      }
      int x = cell.x - gridOrigin.x;
      int y = cell.y - gridOrigin.y;

      //Debug.Log($"SetWalkable - array index: [{x}, {y}], gridOrigin: {gridOrigin}");
      grid[x, y].walkable = walkable;

      //Debug.Log($"SetWalkable - Grid node now walkable={grid[x, y].walkable}");
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

      foreach(var dir in directions){
          Vector3Int checkPos = node.gridPos + dir;

          if (!InBounds(checkPos)) continue;

          int x = checkPos.x - gridOrigin.x;
          int y = checkPos.y - gridOrigin.y;

          neighbors.Add(grid[x, y]);
      }

      return neighbors;
    }

    public void RegisterOccupant(Vector3Int cell, ICombatant unit){
      occupiedTiles[cell] = unit;
    }

    public void UnregisterOccupant(Vector3Int cell){
      if(occupiedTiles.ContainsKey(cell))
          occupiedTiles.Remove(cell);
    }

    public bool IsTileOccupied(Vector3Int cell, GameObject ignore = null){
    if(!occupiedTiles.ContainsKey(cell))
        return false;

    if(ignore != null){
        ICombatant occ = occupiedTiles[cell];
        if(occ != null && occ == ignore.GetComponent<ICombatant>())
            return false;
    }

    return true;
}
  public ICombatant GetOccupant(Vector3Int cell){
      if (occupiedTiles.TryGetValue(cell, out ICombatant unit))
          return unit;

      return null;
  }
    /*void OnDrawGizmos(){
      Gizmos.color = Color.red;

      // start start main grid drawing WorldToViewportPoint
      for(int x = -128; x <= 128; x++){ // These bounds control grid size in x
          for(int y = -128; y <= 128; y++){ // Bounds control size in y direction
            Gizmos.DrawWireCube(new Vector3(x + 0.5f, y + 0.5f, 0),
                new Vector3(1f, 1f, 0)); //Each cell size is 1
          }
      }
    }*/
    //  the above method is being left in as a comment because
    //  it might come in useful for debugging again at some point

    public List<GridNode> GetNodesInRadius(Vector3Int center, int radius)
    {
        List<GridNode> nodes = new List<GridNode>();

        for(int x = -radius; x <= radius; x++)
        {
            for(int y = -radius; y <= radius; y++)
            {
                Vector3Int pos = new Vector3Int(
                    center.x + x,
                    center.y + y,
                    center.z
                );

                float dist = Mathf.Sqrt(x * x + y * y);

                if(dist <= radius)
                {
                    GridNode node = GetNodeFromWorld(GridToWorld(pos));

                    if(node != null)
                        nodes.Add(node);
                }
            }
        }

        return nodes;
    }

    public TileVisual GetTileVisual(Vector3Int pos){
        int x = pos.x - gridOrigin.x;
        int y = pos.y - gridOrigin.y;

        if (x >= 0 && x < width && y >= 0 && y < height)
            return tileVisuals[x, y];

        return null;
    }

    public void HighlightEnemyTiles()
    {
        foreach(var kvp in occupiedTiles)
        {
            if(kvp.Value is Enemy)
            {
                TileVisual tv = GetTileVisual(kvp.Key);

                if(tv != null)
                    tv.Highlight();
            }
        }
    }

    public void ClearAllHighlights()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                tileVisuals[x,y].ClearHighlight();
            }
        }
    }

}
