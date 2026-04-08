using UnityEngine;
using System.Collections.Generic;

public class AOEVisualizer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private GridController grid;
    private int segmentCount = 32; // How many segments to draw the circle with
    private float circleHeight = 0.1f; // Height above ground to draw

    void Start()
    {
        // Create LineRenderer if it doesn't exist
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        grid = FindFirstObjectByType<GridController>();

        // Setup LineRenderer
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1f, 0.5f, 0f, 0.7f); // Orange with transparency
        lineRenderer.endColor = new Color(1f, 0.5f, 0f, 0.7f);
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.sortingOrder = 10;
    }

    public void DrawAOE(Vector3Int gridPos, int radius)
    {
        if (grid == null)
            grid = FindFirstObjectByType<GridController>();

        // Convert grid position to world position
        Vector3 centerWorld = grid.GridToWorld(gridPos);
        centerWorld.z = circleHeight;

        // Get all nodes in radius
        List<GridNode> nodesInRadius = grid.GetNodesInRadius(gridPos, radius);

        // Create positions for the circle outline
        List<Vector3> positions = new List<Vector3>();

        // Draw a circle around the center based on the radius
        float radiusInWorldUnits = radius * 1f; // Adjust multiplier based on your grid size

        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = (i / (float)segmentCount) * 360f * Mathf.Deg2Rad;
            float x = centerWorld.x + Mathf.Cos(angle) * radiusInWorldUnits;
            float y = centerWorld.y + Mathf.Sin(angle) * radiusInWorldUnits;

            positions.Add(new Vector3(x, y, circleHeight));
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        // Highlight affected tiles
        HighlightAOETiles(nodesInRadius);
    }

    public void HideAOE()
    {
        lineRenderer.positionCount = 0;
        ClearAOEHighlights();
    }

    private void HighlightAOETiles(List<GridNode> nodes)
    {
        ClearAOEHighlights();

        foreach (GridNode node in nodes)
        {
            TileVisual visual = grid.GetTileVisual(node.gridPos);
            if (visual != null)
            {
                visual.Highlight();
            }
        }
    }

    private void ClearAOEHighlights()
    {
        if (grid == null)
            return;

        // Clear all highlights
        for (int x = 0; x < grid.grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.grid.GetLength(1); y++)
            {
                TileVisual visual = grid.GetTileVisual(new Vector3Int(x, y, 0));
                if (visual != null)
                {
                    visual.ClearHighlight();
                }
            }
        }
    }
}
