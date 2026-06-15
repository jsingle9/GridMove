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
    [SerializeField] int width = 256;
    [SerializeField] int height = 256;
    [SerializeField] Vector3Int gridOrigin = new Vector3Int(-128, -128, 0);
    public Tilemap obstacleTilemap;

    void Awake()
    {
        grid = new GridNode[width, height];
        tileVisuals = new TileVisual[width, height];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector3Int cellPos = new Vector3Int(
                    x + gridOrigin.x,
                    y + gridOrigin.y,
                    0
                );

                grid[x, y] = new GridNode(cellPos, true);

                Vector3 visualPos = new Vector3(
                    cellPos.x + 0.5f,
                    cellPos.y + 0.5f,
                    0
                );

                GameObject tileObj = Instantiate(
                    tilePrefab,
                    visualPos,
                    Quaternion.identity,
                    transform
                );

                TileVisual visual = tileObj.GetComponent<TileVisual>();
                tileVisuals[x, y] = visual;

              /*  Color tileColor = ((x + y) % 2 == 0)
                    ? new Color(0.35f, 0.65f, 0.35f)
                    : new Color(0.3f, 0.6f, 0.3f); */
                Color tileColor = new Color(1f, 1f, 1f, 0f);
                visual.SetBaseColor(tileColor);
            }
        }
    }

    void Start()
    {
    }

    void Update()
    {
    }

    public GridNode GetNodeFromWorld(Vector3 worldPos)
    {
        Vector3Int cell = WorldToGrid(worldPos);

        if(!InBounds(cell))
            return null;

        int x = cell.x - gridOrigin.x;
        int y = cell.y - gridOrigin.y;

        return grid[x, y];
    }

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y),
            0
        );
    }

    public Vector3 GridToWorld(Vector3Int gridPos)
    {
        return new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0);
    }

    private bool InBounds(Vector3Int cell)
    {
        int x = cell.x - gridOrigin.x;
        int y = cell.y - gridOrigin.y;

        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public bool IsInBounds(Vector3Int cell)
    {
        return InBounds(cell);
    }

    public bool IsWalkable(Vector3Int cell)
    {
        if(!InBounds(cell))
            return false;

        if(occupiedTiles.ContainsKey(cell))
            return false;

        int x = cell.x - gridOrigin.x;
        int y = cell.y - gridOrigin.y;

        return grid[x, y].walkable;
    }

    public void SetWalkable(Vector3Int cell, bool walkable)
    {
        if(!InBounds(cell))
        {
            Debug.LogError($"SetWalkable OUT OF BOUNDS: {cell}");
            return;
        }

        int x = cell.x - gridOrigin.x;
        int y = cell.y - gridOrigin.y;

        grid[x, y].walkable = walkable;
    }

    public bool BlocksLineOfSight(Vector3Int cell)
    {
        if(!InBounds(cell))
            return true;

        int x = cell.x - gridOrigin.x;
        int y = cell.y - gridOrigin.y;

        // For now, anything non-walkable blocks LoS.
        // Occupants do NOT block LoS in this version.
        return !grid[x, y].walkable;
    }

    public bool HasLineOfSight(Vector3Int from, Vector3Int to)
    {
        if(!InBounds(from) || !InBounds(to))
            return false;

        List<Vector3Int> line = GetLineCells(from, to);

        if(line == null || line.Count == 0)
            return false;

        // Skip the start cell and target cell.
        // We only care whether something BETWEEN them blocks sight.
        for(int i = 1; i < line.Count - 1; i++)
        {
            if(BlocksLineOfSight(line[i]))
            {
                Debug.Log($"LoS blocked at {line[i]} between {from} and {to}");
                return false;
            }
        }

        return true;
    }

    // Bresenham grid line between two cells
    public List<Vector3Int> GetLineCells(Vector3Int from, Vector3Int to)
    {
        List<Vector3Int> results = new List<Vector3Int>();

        int x0 = from.x;
        int y0 = from.y;
        int x1 = to.x;
        int y1 = to.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while(true)
        {
            results.Add(new Vector3Int(x0, y0, 0));

            if(x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;

            if(e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if(e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return results;
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

        foreach(var dir in directions)
        {
            Vector3Int checkPos = node.gridPos + dir;

            if(!InBounds(checkPos))
                continue;

            int x = checkPos.x - gridOrigin.x;
            int y = checkPos.y - gridOrigin.y;

            neighbors.Add(grid[x, y]);
        }

        return neighbors;
    }

    public void RegisterOccupant(Vector3Int cell, ICombatant unit)
    {
        occupiedTiles[cell] = unit;
    }

    public void UnregisterOccupant(Vector3Int cell)
    {
        if(occupiedTiles.ContainsKey(cell))
            occupiedTiles.Remove(cell);
    }

    public bool IsTileOccupied(Vector3Int cell, GameObject ignore = null)
    {
        if(!occupiedTiles.ContainsKey(cell))
            return false;

        if(ignore != null)
        {
            ICombatant occ = occupiedTiles[cell];
            if(occ != null && occ == ignore.GetComponent<ICombatant>())
                return false;
        }

        return true;
    }

    public ICombatant GetOccupant(Vector3Int cell)
    {
        if(occupiedTiles.TryGetValue(cell, out ICombatant unit))
            return unit;

        return null;
    }

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

    public TileVisual GetTileVisual(Vector3Int pos)
    {
        int x = pos.x - gridOrigin.x;
        int y = pos.y - gridOrigin.y;

        if(x >= 0 && x < width && y >= 0 && y < height)
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
                tileVisuals[x, y].ClearHighlight();
            }
        }
    }

    public void RegisterCombatant(ICombatant unit)
    {
        if(unit == null) return;

        foreach(Vector3Int cell in unit.GetOccupiedCells())
        {
            occupiedTiles[cell] = unit;
        }
    }

    public void UnregisterCombatant(ICombatant unit)
    {
        if(unit == null) return;

        List<Vector3Int> toRemove = new List<Vector3Int>();

        foreach(var kvp in occupiedTiles)
        {
            if(kvp.Value == unit)
                toRemove.Add(kvp.Key);
        }

        foreach(Vector3Int cell in toRemove)
        {
            occupiedTiles.Remove(cell);
        }
    }

    public bool CanOccupyFootprint(Vector3Int origin, int width, int height, ICombatant ignore = null)
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector3Int cell = origin + new Vector3Int(x, y, 0);

                if(!IsInBounds(cell))
                    return false;

                int gx = cell.x - gridOrigin.x;
                int gy = cell.y - gridOrigin.y;

                if(!grid[gx, gy].walkable)
                    return false;

                ICombatant occ = GetOccupant(cell);
                if(occ != null && occ != ignore)
                    return false;
            }
        }

        return true;
    }

}
