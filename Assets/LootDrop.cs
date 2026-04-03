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
        spriteRenderer.color = Color.yellow;
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);
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
        BoxMover player = collision.GetComponent<BoxMover>();
        if(player != null)
        {
            if (isWeapon && droppedWeapon != null)
            {
                Inventory.Instance.AddWeapon(droppedWeapon);
            }
            else if (!isWeapon && droppedPotion != null)
            {
                Inventory.Instance.AddPotion(droppedPotion);
            }
            Destroy(gameObject);
        }
    }
}
