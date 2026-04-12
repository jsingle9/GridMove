using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Sprite closedDoorSprite;
    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private GridController gridController;

    private SpriteRenderer spriteRenderer;
    private Vector3Int doorGridPosition;
    private bool isOpen = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (gridController == null)
        {
            gridController = FindFirstObjectByType<GridController>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("Door requires a SpriteRenderer component!", this);
        }
    }

    void Start()
    {
        // Get the grid position of this door
        doorGridPosition = gridController.WorldToGrid(transform.position);

        // Set initial state to closed
        CloseDoor();
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        spriteRenderer.sprite = openDoorSprite;

        // Make the tile walkable
        gridController.SetWalkable(doorGridPosition, true);

        Debug.Log($"Door opened at {doorGridPosition}");
    }

    public void CloseDoor()
    {
        if (!isOpen) return;

        isOpen = false;
        spriteRenderer.sprite = closedDoorSprite;

        // Make the tile non-walkable
        gridController.SetWalkable(doorGridPosition, false);

        Debug.Log($"Door closed at {doorGridPosition}");
    }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    public bool IsOpen => isOpen;
}
