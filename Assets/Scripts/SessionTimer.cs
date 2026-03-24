using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SessionTimer : MonoBehaviour
{
    [Header("Session Length")]
    public float sessionLengthSeconds = 300f; // 5 minutes

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject endSessionCanvas;

    [Header("Systems To Disable At End")]
    public VoiceConversationController voiceController;
    public Agent agent;

    [Header("Optional Menus")]
    public GameObject startMenuCanvas;

    private float timeRemaining;
    private bool isRunning;
    private bool hasEnded;

    public GameObject table;
    public GameObject agentChair;
    public GameObject agentModel;

    private void Start()
    {
        ResetTimer();
        UpdateTimerUI();

        if (endSessionCanvas != null)
            endSessionCanvas.SetActive(false);
    }

    private void Update()
    {
        if (!isRunning || hasEnded)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndSession();
        }

        UpdateTimerUI();
    }

    public void StartTimer()
    {
        timeRemaining = sessionLengthSeconds;
        isRunning = true;
        hasEnded = false;

        if (endSessionCanvas != null)
            endSessionCanvas.SetActive(false);

        UpdateTimerUI();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        isRunning = false;
        hasEnded = false;
        timeRemaining = sessionLengthSeconds;
    }

    private void EndSession()
    {
        if (hasEnded)
            return;

        hasEnded = true;
        isRunning = false;

        if (voiceController != null)
        {
            voiceController.isActive = false;
            voiceController.EndListening();
        }

        if (endSessionCanvas != null)
            endSessionCanvas.SetActive(true);

        agentModel.SetActive(false);
        table.SetActive(false);
        agentChair.SetActive(false);

        Debug.Log("Session ended after 5 minutes.");
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        timerText.color = timeRemaining <= 30f ? Color.red : Color.white;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    //public void ReturnToMenu()
    //{
    //    ResetTimer();
    //    UpdateTimerUI();

    //    if (endSessionCanvas != null)
    //        endSessionCanvas.SetActive(false);

    //    if (startMenuCanvas != null)
    //        startMenuCanvas.SetActive(true);
    //}
}