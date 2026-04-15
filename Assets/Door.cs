using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite closedDoorSprite;
    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private GridController gridController;
    private bool hasInitialized = false;

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
        Debug.Log("Door.Start() FIRED!");

        // Wait for GridController and obstacles to initialize
        if(gridController == null)
        {
            gridController = FindFirstObjectByType<GridController>();
        }

        Vector3 spriteCenter = transform.position - new Vector3(0.5f, 0.5f, 0);
        doorGridPosition = gridController.WorldToGrid(spriteCenter);

        isOpen = false;
        spriteRenderer.sprite = closedDoorSprite;

        // Set as non-walkable
        gridController.SetWalkable(doorGridPosition, false);

        Debug.Log($"Door initialized at {doorGridPosition}");
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

    void Update()
    {
      // On first frame after everything initializes, re-check and enforce non-walkable
      if(!Application.isPlaying) return;

      //static bool initialized = false;
      if(!hasInitialized)
      {
          hasInitialized = true;
          // Re-enforce after one frame in case something overwrote it
          if(!isOpen)
          {
              gridController.SetWalkable(doorGridPosition, false);
              Debug.Log($"Door re-enforced non-walkable at {doorGridPosition}");
          }
      }
    }

    public void CloseDoor()
    {
        if (!isOpen) return;

        isOpen = false;
        spriteRenderer.sprite = closedDoorSprite;

        // Make the tile non-walkable
        Debug.Log($"CloseDoor() - Setting {doorGridPosition} to non-walkable");
        gridController.SetWalkable(doorGridPosition, false);
        Debug.Log($"CloseDoor() - Confirmed IsWalkable: {gridController.IsWalkable(doorGridPosition)}");
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

    // IInteractable Implementation
    public void Interact(ICombatant interactor)
    {
        ToggleDoor();
    }

    public Vector3Int GetGridPosition()
    {
        return doorGridPosition;
    }

    public float GetInteractionRange()
    {
        return 1.5f;
    }

    public string GetInteractionPrompt()
    {
        return isOpen ? "close door" : "open door";
    }
}
