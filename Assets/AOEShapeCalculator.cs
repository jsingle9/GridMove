using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class AOEShapeCalculator
{
   /// <summary>
   /// Get all cells affected by a spell given its origin and shape parameters.
   /// </summary>
   public static List<Vector3Int> GetAffectedCells(
       Vector3Int origin,
       SpellAreaShape shape,
       int radius,           // Used for Area (circle/square)
       int coneLength,       // Used for Cone
       float coneAngleDegrees,
       int coneMinRange,
       Vector3Int coneDirection, // Direction the cone fires
       GridController grid
   )
   {
       List<Vector3Int> cells = new();

       if (shape == SpellAreaShape.Single)
       {
           cells.Add(origin);
       }
       else if (shape == SpellAreaShape.Area)
       {
           cells = GetCircleAOE(origin, radius, grid);
       }
       else if (shape == SpellAreaShape.Cone)
       {
           cells = GetConeAOE(origin, coneDirection, coneLength, coneAngleDegrees, coneMinRange, grid);
       }

       return cells;
   }

   /// <summary>
   /// Circle AOE using Euclidean distance (smooth circles).
   /// Alternative: use Manhattan distance for diamond shapes.
   /// </summary>
   private static List<Vector3Int> GetCircleAOE(Vector3Int origin, int radius, GridController grid)
   {
       List<Vector3Int> cells = new();

       for (int x = origin.x - radius; x <= origin.x + radius; x++)
       {
           for (int y = origin.y - radius; y <= origin.y + radius; y++)
           {
               Vector3Int cell = new Vector3Int(x, y, 0);

               if (!grid.IsInBounds(cell)) continue;

               // Euclidean distance (circle)
               float distance = Vector3Int.Distance(origin, cell);
               if (distance <= radius + 0.5f)
               {
                   cells.Add(cell);
               }
           }
       }

       return cells;
   }

   /// <summary>
   /// Cone AOE emanating from origin in a direction.
   /// </summary>
   private static List<Vector3Int> GetConeAOE(
       Vector3Int origin,
       Vector3Int direction,
       int length,
       float angleDegrees,
       int minRange,
       GridController grid
   )
   {
       List<Vector3Int> cells = new();

       // Normalize direction
       if (direction.magnitude == 0) return cells;

       Vector2 normDir = new Vector2(direction.x, direction.y).normalized;
       float halfAngleRad = angleDegrees * 0.5f * Mathf.Deg2Rad;

       for (int distance = minRange; distance < minRange + length; distance++)
       {
           // Points along the cone's axis at this distance
           Vector3Int axisPoint = origin + new Vector3Int(
               Mathf.RoundToInt(normDir.x * distance),
               Mathf.RoundToInt(normDir.y * distance),
               0
           );

           // Add the axis point
           if (grid.IsInBounds(axisPoint))
               cells.Add(axisPoint);

           // Spread width: increases with distance
           int spreadWidth = Mathf.Max(1, Mathf.RoundToInt(distance * Mathf.Tan(halfAngleRad)));

           // Add perpendicular spread
           for (int spread = -spreadWidth; spread <= spreadWidth; spread++)
           {
               Vector2 perpDir = new Vector2(-normDir.y, normDir.x); // Rotate 90°
               Vector3Int spreadPoint = origin + new Vector3Int(
                   Mathf.RoundToInt(normDir.x * distance + perpDir.x * spread),
                   Mathf.RoundToInt(normDir.y * distance + perpDir.y * spread),
                   0
               );

               if (grid.IsInBounds(spreadPoint))
                   cells.Add(spreadPoint);
           }
       }

       // Deduplicate
       return new List<Vector3Int>(new HashSet<Vector3Int>(cells));
   }

   /// <summary>
   /// Line/ray AOE in a direction (useful for lightning, etc.).
   /// </summary>
   public static List<Vector3Int> GetLineAOE(Vector3Int origin, Vector3Int direction, int length, GridController grid)
   {
       List<Vector3Int> cells = new();

       if (direction.magnitude == 0) return cells;

       Vector2 normDir = new Vector2(direction.x, direction.y).normalized;

       for (int distance = 0; distance < length; distance++)
       {
           Vector3Int point = origin + new Vector3Int(
               Mathf.RoundToInt(normDir.x * distance),
               Mathf.RoundToInt(normDir.y * distance),
               0
           );

           if (grid.IsInBounds(point))
               cells.Add(point);
       }

       return cells;
   }
}
