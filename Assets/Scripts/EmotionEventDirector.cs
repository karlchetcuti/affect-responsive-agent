using UnityEngine;

public class EmotionEventDirector : MonoBehaviour
{
    [Header("References")]
    public ExperimentManager experiment;
    public EmotionalStateController emotionController;

    [Header("Event Handlers")]
    public LightFlickerEvent lightFlickerEvent;
    public ThunderEvent thunderEvent;
    public HeartbeatLoopEvent heartbeatLoopEvent;
    public WallKnockEvent wallKnockEvent;
    public LampExplosionEvent lampExplosionEvent;

    [Header("Global Settings")]
    public float globalCooldownSeconds = 12f;

    private float nextAllowedEventTime;

    private void OnEnable()
    {
        if (emotionController != null)
            emotionController.OnStateUpdated += OnStateUpdated;
    }

    private void OnDisable()
    {
        if (emotionController != null)
            emotionController.OnStateUpdated -= OnStateUpdated;
    }

    private void OnStateUpdated(DimensionalEmotionData state)
    {
        if (experiment != null && !experiment.IsAdaptive)
            return;

        if (heartbeatLoopEvent != null)
            heartbeatLoopEvent.UpdateLoopState(state);

        if (Time.time < nextAllowedEventTime)
            return;

        bool triggered = false;

        float negative = Mathf.Clamp01((-state.valence + 1f) * 0.5f);
        float activated = Mathf.Clamp01((state.arousal + 1f) * 0.5f);
        float submissive = Mathf.Clamp01((-state.dominance + 1f) * 0.5f);
        float dominant = Mathf.Clamp01((state.dominance + 1f) * 0.5f);

        float fearLike = negative * activated * submissive;
        float angerLike = negative * activated * dominant;

        if (!triggered && fearLike > 0.30f && wallKnockEvent != null)
            triggered = wallKnockEvent.TryTrigger(fearLike);

        if (!triggered && fearLike > 0.45f && lightFlickerEvent != null)
            triggered = lightFlickerEvent.TryTrigger(fearLike);

        if (!triggered && angerLike > 0.35f && thunderEvent != null)
            triggered = thunderEvent.TryTrigger(angerLike);

        if (!triggered && angerLike > 0.55f && lampExplosionEvent != null)
            triggered = lampExplosionEvent.TryTrigger(angerLike);

        if (triggered)
            nextAllowedEventTime = Time.time + globalCooldownSeconds;
    }
}