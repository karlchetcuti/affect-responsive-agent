#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class WindowsDictationStt : MonoBehaviour, ISttService
{
    private DictationRecognizer recognizer;

    private Action<string> onFinalText;
    private Action<string> onPartialText;
    private Action<string> onError;

    public bool IsListening { get; private set; }
    private bool stopRequested;

    private string latestHypothesis = "";
    private string latestFinalResult = "";

    [Header("Timeouts")]
    public float initialSilenceTimeoutSeconds = 5f;
    public float autoSilenceTimeoutSeconds = 10f;

    private void Awake()
    {
        recognizer = new DictationRecognizer();
        recognizer.InitialSilenceTimeoutSeconds = initialSilenceTimeoutSeconds;
        recognizer.AutoSilenceTimeoutSeconds = autoSilenceTimeoutSeconds;

        recognizer.DictationHypothesis += HandleHypothesis;
        recognizer.DictationResult += HandleResult;
        recognizer.DictationComplete += HandleComplete;
        recognizer.DictationError += HandleError;
    }

    public void StartListening(
        Action<string> onFinalText,
        Action<string> onPartialText = null,
        Action<string> onError = null
        )
    {
        if (recognizer == null)
        {
            this.onError?.Invoke("Dictation recognizer is not initialized.");
            return;
        }

        if (recognizer.Status == SpeechSystemStatus.Running || IsListening)
            return;

        this.onFinalText = onFinalText;
        this.onPartialText = onPartialText;
        this.onError = onError;
        stopRequested = false;
        latestHypothesis = "";
        latestFinalResult = "";

        try
        {
            recognizer.Start();
            IsListening = true;
            Debug.Log("Dictation started.");
        }
        catch (Exception e )
        {
            IsListening = false;
            this.onError?.Invoke($"Failed to start dictation: {e.Message}");
        }
    }

    public void StopListening()
    {
        if (recognizer == null)
            return;

        if (stopRequested)
            return;

        if (!IsListening)
            return;

        if (recognizer.Status != SpeechSystemStatus.Running)
        {
            IsListening = false;
            return;
        }

        stopRequested = true;

        try
        {
            recognizer.Stop();
            Debug.Log("Dictation stop requested.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Dictation Stop failed safely: {e.Message}");
            IsListening = false;
            stopRequested = false;
        }
    }

    private void HandleHypothesis(string text)
    {
        latestHypothesis = text;
        Debug.Log($"Dictation hypothesis: {text}");
        onPartialText?.Invoke(text);
    }

    private void HandleResult(string text, ConfidenceLevel confidence)
    {
        latestFinalResult = text;
        Debug.Log($"Dictation result: {text} ({confidence})");

        //IsListening = false;
        //stopRequested = false;
        //onFinalText?.Invoke(text);
    }

    private void HandleComplete(DictationCompletionCause cause)
    {
        Debug.Log($"Dictation complete: {cause}");

        IsListening = false;
        stopRequested = false;

        string bestText = "";

        if (!string.IsNullOrWhiteSpace(latestFinalResult))
        {
            bestText = latestFinalResult.Trim();
            Debug.Log($"Using final dictation result: {bestText}");
        }
        else if (!string.IsNullOrWhiteSpace(latestHypothesis))
        {
            bestText = latestHypothesis.Trim();
            Debug.LogWarning($"No final dictation result received, falling back to hypothesis: {bestText}");
        }

        if (!string.IsNullOrWhiteSpace(bestText))
        {
            onFinalText?.Invoke(bestText);
            return;
        }

        if (cause != DictationCompletionCause.Complete && cause != DictationCompletionCause.TimeoutExceeded)
        {
            onError?.Invoke($"Dictation ended: {cause}");
        }
        else
        {
            onError?.Invoke("No speech text captured.");
        }
    }

    private void HandleError(string error, int hresult)
    {
        Debug.LogWarning($"Dictation error: {error} ({hresult})");
        IsListening = false;
        stopRequested = false;
        onError?.Invoke($"Dictation error: {error} ({hresult})");
    }

    //private void OnApplicationFocus(bool hasFocus)
    //{
    //    if (!hasFocus)
    //    {
    //        Debug.Log("Application lost focus; ending dictation safely.");
    //        StopListening();
    //    }
    //}

    //private void OnApplicationPause(bool pauseStatus)
    //{
    //    if (pauseStatus)
    //    {
    //        Debug.Log("Application paused; ending dictation safely.");
    //        StopListening();
    //    }
    //}

    private void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.DictationHypothesis -= HandleHypothesis;
            recognizer.DictationResult -= HandleResult;
            recognizer.DictationComplete -= HandleComplete;
            recognizer.DictationError -= HandleError;

            try
            {
                if (recognizer.Status == SpeechSystemStatus.Running)
                    recognizer.Stop();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Dictation stop during destroy failed safely: {e.Message}");
            }

            recognizer.Dispose();
            recognizer = null;
        }
    }
}
#endif