using UnityEngine;

public class ApplySelectedClassAtStart : MonoBehaviour
{
    [SerializeField] private ClassTemplateDatabase classDb;
    [SerializeField] private SpriteRenderer playerSpriteRenderer; // or Image for UI portrait
    [SerializeField] private MonoBehaviour combatantComponent;    // assign component implementing ICombatant

    private void Start()
    {
        var classId = PlayerPrefs.GetString("SelectedClassId", "fighter");
        var template = classDb.Get(classId);
        if (template == null)
        {
            Debug.LogError($"No template for classId={classId}");
            return;
        }

        CharacterSheet sheet = CharacterFactory.CreateFromTemplate(template);
        // TODO: store sheet in your game state/save system

        if (playerSpriteRenderer != null && template.classSprite != null)
            playerSpriteRenderer.sprite = template.classSprite;

        if (combatantComponent is ICombatant combatant)
            ClassLoadoutApplier.ApplyTo(combatant, template);
    }
}
