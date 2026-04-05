using UnityEngine;

public class LootDrop : MonoBehaviour
{
    private Weapon droppedWeapon;
    private HealingPotion droppedPotion;
    private bool isWeapon;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        circleCollider = GetComponent<CircleCollider2D>();
        if(circleCollider == null)
            circleCollider = gameObject.AddComponent<CircleCollider2D>();

        circleCollider.radius = 0.3f;
        circleCollider.isTrigger = true;

        // Visual feedback
        spriteRenderer.sortingOrder = 3;
        spriteRenderer.color = new Color(0.68f, 0.85f, 1f);
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);
spriteRenderer.sprite = Resources.Load<Sprite>("karsiori/Pixel Chest Pack - Animated/Sprites/Wooden Chest 1/Wooden Chest 1 Sprites/Wooden Chest 1 - frame  01");
        // Built-in Unity sprite

        Debug.Log($"LootDrop ready at {transform.position}");
        Debug.Log($"BoxMover layer: {LayerMask.LayerToName(gameObject.layer)}");
        Debug.Log($"LootDrop sprite renderer: {spriteRenderer}");
        Debug.Log($"LootDrop sprite: {spriteRenderer.sprite}");
        Debug.Log($"LootDrop color: {spriteRenderer.color}");
        Debug.Log($"LootDrop sorting order: {spriteRenderer.sortingOrder}");
        Debug.Log($"LootDrop position: {transform.position}");
        Debug.Log($"LootDrop scale: {transform.localScale}");
    }

    public void SetWeapon(Weapon weapon)
    {
        droppedWeapon = weapon;
        isWeapon = true;
        Debug.Log($"Loot drop: {weapon.WeaponName}");
    }

    public void SetPotion(HealingPotion potion)
    {
        droppedPotion = potion;
        isWeapon = false;
        Debug.Log($"Loot drop: Healing Potion");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("LootDrop triggered by: " + collision.gameObject.name);

        BoxMover player = collision.GetComponent<BoxMover>();
        if(player != null)
        {
            Debug.Log("Player detected! Attempting to add loot...");

            if (isWeapon && droppedWeapon != null)
            {
                Debug.Log($"Adding weapon: {droppedWeapon.WeaponName}");
                Inventory.Instance.AddWeapon(droppedWeapon);
            }
            else if (!isWeapon && droppedPotion != null)
            {
                Debug.Log("Adding potion to inventory");
                Inventory.Instance.AddPotion(droppedPotion);
            }
            else
            {
                Debug.LogError("No loot to add! isWeapon: " + isWeapon);
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("No BoxMover found on collision object");
        }
    }
}
