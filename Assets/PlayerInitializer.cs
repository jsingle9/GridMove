using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] private MonoBehaviour playerCombatant;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    private void Start()
    {
        string classId = PlayerPrefs.GetString("SelectedClassId", "fighter");

        Debug.Log($"PlayerInitializer read class ID: {classId}");

        CharacterSheet characterSheet =
            CharacterFactory.CreateCharacterByClass(classId);

        if (characterSheet == null)
        {
            Debug.LogError("CharacterFactory returned a null CharacterSheet.");
            return;
        }

        Debug.Log($"Created character: {characterSheet.CharacterName}");
        Debug.Log($"Created class: {characterSheet.ClassId}");

        if (playerCombatant is ICombatant combatant)
        {
            ApplyClassLoadout(combatant, classId);
        }

        ClassTemplateDatabase database = CharacterFactory.ClassDb;

        if (database != null)
        {
            ClassTemplate template = database.Get(classId);

            if (template != null &&
                playerSpriteRenderer != null &&
                template.classSprite != null)
            {
                playerSpriteRenderer.sprite = template.classSprite;
            }
        }

        // Store characterSheet wherever your player/game state expects it.
        // Example:
        // GameState.CurrentCharacter = characterSheet;
    }

    private void ApplyClassLoadout(ICombatant combatant, string classId)
    {
        ClassTemplateDatabase database = CharacterFactory.ClassDb;

        if (database == null)
        {
            Debug.LogWarning("No ClassTemplateDatabase assigned.");
            return;
        }

        ClassTemplate template = database.Get(classId);

        if (template == null)
        {
            Debug.LogWarning($"No ClassTemplate found for class ID '{classId}'.");
            return;
        }

        ClassLoadoutApplier.ApplyTo(
            combatant,
            template,
            playerSpriteRenderer
        );
    }
}
