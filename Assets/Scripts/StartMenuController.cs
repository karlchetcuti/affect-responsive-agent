using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public Agent agent;
    public VoiceConversationController voice;
    public SessionTimer sessionTimer;
    public GameObject table;
    public GameObject agentChair;
    public GameObject agentModel;
    public GameObject pauseCanvas;

    public void StartExperience()
    {
        menuCanvas.SetActive(false);
        pauseCanvas.SetActive(true);

        if (agent != null)
            agent.StartNewSession();

        Debug.Log("Experience started");
    }
}