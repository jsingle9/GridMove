using UnityEngine;

public class LootDrop : MonoBehaviour
{

    private Item droppedItem;
    [SerializeField] private Potion defaultPotionAsset;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        circleCollider = GetComponent<CircleCollider2D>();
        if(circleCollider == null)
            circleCollider = gameObject.AddComponent<CircleCollider2D>();

        circleCollider.radius = 0.3f;
        circleCollider.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.isKinematic = true;
        }
    }

    void Start()
    {
        // Load and assign sprite
        spriteRenderer.sprite = Resources.Load<Sprite>("karsiori/Pixel Chest Pack - Animated/Sprites/Wooden Chest 1/Wooden Chest 1 Sprites/Wooden Chest 1 - frame  01");

        if(spriteRenderer.sprite == null)
        {
            Debug.LogError("Failed to load sprite! Check path and Resources folder structure");
        }

        // Visual feedback
        spriteRenderer.sortingOrder = 3;
        spriteRenderer.color = new Color(0.68f, 0.85f, 1f);
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        GridController grid = FindFirstObjectByType<GridController>();
        if(grid != null)
        {
            Vector3Int cell = grid.WorldToGrid(transform.position);
            grid.SetWalkable(cell, true);
        }

        // If no item was set via SetItem/SetPotion, default to the potion asset
        if (droppedItem == null && defaultPotionAsset != null)
        {
            droppedItem = defaultPotionAsset;
        }


        Debug.Log($"LootDrop ready at {transform.position}");
        Debug.Log($"LootDrop sprite: {spriteRenderer.sprite}");
        Debug.Log($"LootDrop position: {transform.position}");
    }

    public void SetItem(Item item)
    {
        droppedItem = item;
        Debug.Log($"Loot drop: {(item != null ? item.itemName : "null")}");
    }

    public void SetPotion(HealingPotion potion)
    {
        droppedItem = potion;
        Debug.Log("Loot drop: Healing Potion");
    }

    public void SetWeapon(Weapon weapon)
    {
        if (weapon == null)
            return;

        WeaponItem weaponItem = ScriptableObject.CreateInstance<WeaponItem>();
        weaponItem.itemId = weapon.WeaponName.ToLowerInvariant().Replace(" ", "_");
        weaponItem.itemName = weapon.WeaponName;
        weaponItem.damageDice = weapon.DamageDice;
        weaponItem.damageBonus = weapon.DamageBonus;
        weaponItem.weaponType = weapon.WeaponType;
        weaponItem.range = weapon.WeaponType == WeaponType.Ranged ? 6 : 1;

        droppedItem = weaponItem;
        Debug.Log($"Loot drop converted: {weapon.WeaponName}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("LootDrop triggered by: " + collision.gameObject.name);

        BoxMover player = collision.GetComponent<BoxMover>();
        if (player != null)
        {
            Debug.Log("Player detected! Attempting to add loot...");

            if (droppedItem != null)
            {
                player.AddItem(droppedItem);
                Debug.Log($"Added item: {droppedItem.itemName}");
            }
            else
            {
                Debug.LogError("No loot item to add!");
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("No BoxMover found on collision object");
        }
    }
}
