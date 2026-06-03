using System.Collections;
using TMPro;
using UnityEngine;

public class IntroNarrationController : MonoBehaviour
{
    [SerializeField] private GameObject narrationPanel;
    [SerializeField] private TextMeshProUGUI narrationText;
    [SerializeField] private float displayDuration = 4f;

    [TextArea(2, 4)]
    [SerializeField] private string introMessage =
        "You wake up in a cold, dark dungeon. You're not quite sure how you got here...";

    private IEnumerator Start()
    {
        narrationPanel.SetActive(true);
        narrationText.text = introMessage;

        yield return new WaitForSeconds(displayDuration);

        narrationPanel.SetActive(false);
    }
}
