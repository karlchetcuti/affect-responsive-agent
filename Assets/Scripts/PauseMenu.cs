using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class PauseMenu : MonoBehaviour
{
    public GameObject menuCanvas;
    public VoiceConversationController voice;
    public SessionTimer sessionTimer;
    public GameObject table;
    public GameObject agentChair;
    public GameObject agentModel;

    public void CloseMenu()
    {
        menuCanvas.SetActive(false);
        agentModel.SetActive(true);
        table.SetActive(true);
        agentChair.SetActive(true);

        if (voice != null)
            voice.isActive = true;

        if (sessionTimer != null)
            sessionTimer.StartTimer();

        Debug.Log("Pause menu closed.");
    }
}