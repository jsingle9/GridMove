using UnityEngine;
using System.Collections.Generic;

public class GameSceneBootstrap : MonoBehaviour
{
    [SerializeField] private ClassTemplateDatabase classDb;
    private BoxMover[] boxMovers;

    private void Awake()
    {
        Debug.Log("GameSceneBootstrap.Awake() fired!");

        // Find the 3 inactive BoxMover objects in this scene
        boxMovers = FindObjectsOfType<BoxMover>(includeInactive: true);
        Debug.Log($"Found {boxMovers.Length} BoxMover objects in scene");

        if (boxMovers.Length < 3)
        {
            Debug.LogError($"Expected 3 BoxMover objects, found {boxMovers.Length}");
            return;
        }

        SetupParty();
    }

    private void SetupParty()
    {
        Debug.Log("SetupParty() starting...");

        List<BoxMover> party = new List<BoxMover>();
        string[] classIds = { "fighter", "mystic", "mage" };

        for (int i = 0; i < classIds.Length && i < boxMovers.Length; i++)
        {
            string classId = classIds[i];
            BoxMover boxMover = boxMovers[i];

            Debug.Log($"Setting up {classId}...");

            var template = classDb.Get(classId);
            if (template == null)
            {
                Debug.LogError($"No template for {classId}");
                continue;
            }

            CharacterSheet sheet = CharacterFactory.CreateFromTemplate(template);
            boxMover.SetCharacterSheet(sheet);
            boxMover.gameObject.SetActive(true);

            party.Add(boxMover);
            Debug.Log($"Activated {classId}");
        }

        Debug.Log($"PartyManager.Instance = {PartyManager.Instance}");
        if (PartyManager.Instance != null)
        {
            PartyManager.Instance.SetPartyMembers(party);
            Debug.Log($"Party set up with {party.Count} members");
        }
        else
        {
            Debug.LogError("PartyManager.Instance is NULL!");
        }
    }
}
