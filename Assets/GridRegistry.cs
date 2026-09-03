using UnityEngine;

/// <summary>
/// Tiny global access helper for abilities/utilities that need grid.
/// Set once from scene bootstrap / player Awake.
/// </summary>
public static class GridRegistry
{
    public static GridController Grid { get; private set; }

    public static void Set(GridController grid)
    {
        Grid = grid;
    }
}
