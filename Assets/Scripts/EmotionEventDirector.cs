using System.Collections;
using UnityEngine;

public class EmotionEventDirector : MonoBehaviour
{
    [Header("References")]
    //public Agent agent;
    public ExperimentManager experiment;
    public EmotionalStateController emotionController;

    [Header("Event Handlers")]
    public LightFlickerEvent lightFlickerEvent;
    public ChairScrapeEvent chairScrapeEvent;
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

        float negativeValence = Mathf.Clamp01(-state.valence);

        // Slight anxious/stressed -> wall knock
        if (!triggered &&
            state.stress >= 0.55f &&
            state.stress < 0.80f &&
            state.arousal >= 0.45f &&
            state.dominance < 0.45f &&
            wallKnockEvent != null)
        {
            triggered = wallKnockEvent.TryTrigger(Mathf.Clamp01((state.stress + state.arousal) * 0.5f));
        }

        // High anxious/stressed -> light flicker
        if (!triggered &&
            state.stress > 0.78f &&
            state.arousal > 0.60f &&
            state.dominance < 0.50f &&
            lightFlickerEvent != null)
        {
            triggered = lightFlickerEvent.TryTrigger(Mathf.Clamp01((state.stress + state.arousal) * 0.5f));
        }

        // Angry/defensive -> chair scrape
        if (!triggered &&
            state.arousal > 0.82f &&
            state.dominance > 0.48f &&
            negativeValence > 0.45f &&
            chairScrapeEvent != null)
        {
            triggered = chairScrapeEvent.TryTrigger(Mathf.Clamp01((state.arousal + state.dominance) * 0.5f));
        }

        // High angry/defensive -> lamp explosion
        if (!triggered &&
            state.arousal > 0.93f &&
            state.dominance > 0.62f &&
            state.stress > 0.80f &&
            negativeValence > 0.60f &&
            lampExplosionEvent != null)
        {
            triggered = lampExplosionEvent.TryTrigger(Mathf.Clamp01((state.arousal + state.dominance + state.stress) / 3f));
        }

        if (triggered)
            nextAllowedEventTime = Time.time + globalCooldownSeconds;
    }

    //private void OnEmotionParsed(EmotionParser.EmotionData emo)
    //{
    //    if (experiment != null && !experiment.IsAdaptive)
    //        return;

    //    if (Time.time < nextAllowedEventTime)
    //        return;

    //    string emotion = NormalizeEmotion(emo.emotion);
    //    float intensity = Mathf.Clamp01(emo.intensity);

    //    bool triggered = false;

    //    switch (emotion)
    //    {
    //        case "anxious":
    //            if (lightFlickerEvent != null)
    //                triggered = lightFlickerEvent.TryTrigger(intensity);
    //            break;

    //        case "angry":
    //            if (audioStingerEvent != null)
    //                triggered = audioStingerEvent.TryTrigger(intensity);
    //            break;

    //        case "sad":
    //            if (folderSlideEvent != null)
    //                triggered = folderSlideEvent.TryTrigger(intensity);
    //            break;
    //    }

    //    if (triggered)
    //        nextAllowedEventTime = Time.time + globalCooldownSeconds;
    //}

    //private string NormalizeEmotion(string emotion)
    //{
    //    if (string.IsNullOrWhiteSpace(emotion))
    //        return "anxious";

    //    switch (emotion.Trim().ToLowerInvariant())
    //    {
    //        case "anxious":
    //        case "angry":
    //        case "sad":
    //        case "happy":
    //            return emotion.Trim().ToLowerInvariant();
    //        default:
    //            return "anxious";
    //    }
    //}
}