using UnityEngine;
using System.Collections.Generic;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private float defaultInteractionRange = 1.5f;
    private List<IInteractable> interactablesInRange = new List<IInteractable>();
    private IInteractable currentTarget;
    private GridController grid;
    private BoxMover player;
    public static InteractionSystem Instance;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    void Start()
    {
        grid = FindFirstObjectByType<GridController>();
        player = FindFirstObjectByType<BoxMover>();
    }

    void Update()
    {
        UpdateNearbyInteractables();
    }

    void UpdateNearbyInteractables()
    {
        interactablesInRange.Clear();

        if(player == null || grid == null) return;

        Vector3 playerWorldPos = player.GetWorldPosition();
        Vector3Int playerGridPos = grid.WorldToGrid(playerWorldPos);

        // Find all Door objects (or other IInteractables)
        Door[] allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);

        foreach(Door door in allDoors)
        {
            Vector3Int doorGridPos = door.GetGridPosition();

            int distance = Mathf.Abs(playerGridPos.x - doorGridPos.x) +
                          Mathf.Abs(playerGridPos.y - doorGridPos.y);

            if(distance <= (int)door.GetInteractionRange())
            {
                interactablesInRange.Add(door);
            }
        }

        // Set closest as current target
        currentTarget = interactablesInRange.Count > 0 ? interactablesInRange[0] : null;

        if(currentTarget != null)
        {
            Debug.Log($"Press E to {currentTarget.GetInteractionPrompt()}");
        }
    }

    public void AttemptInteraction()
    {
        if(currentTarget == null)
        {
            Debug.Log("Nothing to interact with");
            return;
        }

        currentTarget.Interact(player);
    }
}
