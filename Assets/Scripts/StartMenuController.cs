using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public Agent agent;
    public VoiceConversationController voice;
    public SessionTimer sessionTimer;

    public void StartExperience()
    {
        // Hide menu
        menuCanvas.SetActive(false);

        // Reset session
        if (agent != null)
            agent.StartNewSession();

        if (voice != null)
            voice.isActive = true;

        if (sessionTimer != null)
            sessionTimer.StartTimer();

        Debug.Log("Experience started");
    }
}