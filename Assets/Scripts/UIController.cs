using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public InputField input;
    public Agent ai;

    [Header("Output")]
    public Text agentText;

    private void Awake()
    {
        ai.OnAgentTextUpdated += UpdateAgentText;
        ai.OnAgentTurnComplete += FinalizeAgentText;
    }

    // Send input text to AI model
    public void Send()
    {
        string msg = input.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        agentText.text = "";
        ai.Ask(msg);

        input.text = "";
        input.ActivateInputField();
    }

    private void UpdateAgentText(string fullSoFar)
    {
        agentText.text = fullSoFar;
    }

    private void FinalizeAgentText(string final)
    {
        agentText.text = final;
    }
}