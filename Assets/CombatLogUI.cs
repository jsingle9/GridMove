using UnityEngine;
using TMPro;

public class CombatLogUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logText;

    private void OnEnable()
    {
        CombatEvents.OnLog += HandleLog;
    }

    private void OnDisable()
    {
        CombatEvents.OnLog -= HandleLog;
    }

    private void HandleLog(string message)
    {
        if (logText == null) return;
        logText.text += $"\n{message}";
    }

    // Optional helper for a clear button
    public void ClearLog()
    {
        if (logText != null) logText.text = "";
    }
}
