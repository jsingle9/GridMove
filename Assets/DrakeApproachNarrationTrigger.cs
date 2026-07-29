using UnityEngine;

public class DrakeApproachNarrationTrigger : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform drake;
    [SerializeField] private float warningRadius = 10f;
    [SerializeField] private string warningText = "You aprroach the lair of the Fire Drake, you look at the tiny short sword you carry... That drake is far too dangerous right now... You think, 'I should find a better weapon first'.";

    private bool hasPlayed = false;

    void Update()
    {
        if (hasPlayed || player == null || drake == null) return;
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Combat) return;

        float dist = Vector3.Distance(player.position, drake.position);
        if (dist <= warningRadius)
        {
            hasPlayed = true;
            CombatUIManager.Instance?.AddLog(warningText);
            Debug.Log("[Narration] " + warningText);
        }
    }
}
