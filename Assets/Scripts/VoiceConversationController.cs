using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class VoiceConversationController : MonoBehaviour
{
    [Header("References")]
    public Agent agent;
    public MonoBehaviour sttBehaviour;
    public MonoBehaviour ttsBehaviour;

    public Text partialTranscriptText;

    [Header("XR Input")]
    public InputActionReference pushToTalkAction;

    private ISttService stt;
    private ITtsService tts;

    private bool waitingForAgent;
    public bool isActive = false;

    private void Awake()
    {
        stt = sttBehaviour as ISttService;
        tts = ttsBehaviour as ITtsService;
    }
    private void OnEnable()
    {
        if (agent != null)
            agent.OnAgentTurnComplete += HandleAgentTurnComplete;

        if (pushToTalkAction != null && pushToTalkAction.action != null)
        {
            pushToTalkAction.action.started += OnPushToTalkStarted;
            pushToTalkAction.action.canceled += OnPushToTalkCanceled;
            pushToTalkAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (agent != null)
            agent.OnAgentTurnComplete -= HandleAgentTurnComplete;

        if (pushToTalkAction != null && pushToTalkAction.action != null)
        {
            pushToTalkAction.action.started -= OnPushToTalkStarted;
            pushToTalkAction.action.canceled -= OnPushToTalkCanceled;
            pushToTalkAction.action.Disable();
        }
    }

    private void OnPushToTalkStarted(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;
        Debug.Log("PTT START");
        BeginListening();
    }

    private void OnPushToTalkCanceled(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;
        Debug.Log("PTT END");
        EndListening();
    }

    public void BeginListening()
    {
        if (tts != null && tts.IsSpeaking)
            tts.StopSpeaking();

        if (waitingForAgent) return;
        if (stt == null || stt.IsListening) return;

        SetStatus("Listening...");
        if (partialTranscriptText != null) partialTranscriptText.text = "";

        stt.StartListening(
            onFinalText: OnPlayerSpeechFinal,
            onPartialText: OnPlayerSpeechPartial,
            onError: OnSpeechError
        );
    }

    public void EndListening()
    {
        if (stt != null && stt.IsListening)
        {
            SetStatus("Processing speech...");
            stt.StopListening();
        }
    }

    private void OnPlayerSpeechPartial(string partial)
    {
        if (partialTranscriptText != null)
            partialTranscriptText.text = partial;
    }

    private void OnPlayerSpeechFinal(string text)
    {
        Debug.Log("Final Text: " + text);

        if (string.IsNullOrWhiteSpace(text))
        {
            SetStatus("No speech detected.");
            return;
        }

        waitingForAgent = true;
        SetStatus("Thinking...");

        if (partialTranscriptText != null)
            partialTranscriptText.text = text;

        agent.Ask(text);
    }

    private void HandleAgentTurnComplete(string finalAgentText)
    {
        waitingForAgent = false;

        if (tts == null)
        {
            SetStatus("Ready");
            return;
        }

        StartCoroutine(
            tts.Speak(
                finalAgentText,
                onComplete: () => SetStatus("Ready"),
                onError: err => SetStatus(err)
            )
        );
    }

    private void OnSpeechError(string error)
    {
        waitingForAgent = false;
        SetStatus(error);
    }

    private void SetStatus(string text)
    {
        Debug.Log(text);
    }
}
