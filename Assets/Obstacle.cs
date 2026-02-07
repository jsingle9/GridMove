using UnityEngine;

public class Obstacle : MonoBehaviour
{
    GridController grid;
    //public GameObject box;
    //public GameObject obstacle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      grid = FindFirstObjectByType<GridController>();

      if(grid == null){

        Debug.LogError($"Obstacle '{name}' has NO GridController in scene!", this);
          return;

      }
          Vector3Int cell = grid.WorldToGrid(transform.position);
          grid.SetWalkable(cell, false);
          Debug.Log($"Obstacle '{name}' registered at {cell}", this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
