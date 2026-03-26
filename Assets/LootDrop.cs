using UnityEngine;

public class LootDrop : MonoBehaviour
{
    private Weapon droppedWeapon;
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
        Debug.Log($"Loot drop: {weapon.WeaponName}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        BoxMover player = collision.GetComponent<BoxMover>();
        if(player != null && droppedWeapon != null)
        {
            player.EquipWeapon(droppedWeapon);
            Destroy(gameObject);
        }
    }
}
