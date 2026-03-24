using System;
using UnityEngine;
using UnityEngine.Audio;

public class EnvironmentDirector : MonoBehaviour
{
    [Header("References")]
    //public Agent agent;
    public ExperimentManager experiment;
    //public EmotionToEnvironmentMap map;
    public EmotionalStateController emotionController;

    [Header("Scene Targets")]
    public Light[] mainLights;
    public AudioMixer audioMixer;
    public string ambienceVolumeParameter = "AmbienceVolume";

    [Header("Baseline")]
    public Color baselineLightColor = new Color(0.95f, 0.95f, 1f);
    public float baselineLightIntensity = 1.2f;
    public Color baselineFogColor = new Color(0.5f, 0.5f, 0.55f);
    public float baselineFogDensity = 0.005f;
    public float baselineAmbienceVolume = 0.7f;

    [Header("Ranges")]
    public float maxExtraFogDensity = 0.08f;
    public float maxExtraLightIntensityShift = 1.2f;

    [Header("Smoothing")]
    [Range(0.1f, 10f)]
    public float transitionSeconds = 6f; // maybe lower to 3f

    // Current rendered values
    private Color currentLightColor;
    private float currentLightIntensity;
    private Color currentFogColor;
    private float currentFogDensity;
    private float currentAmbienceVolume;

    // Target values
    private Color targetLightColor;
    private float targetLightIntensity;
    private Color targetFogColor;
    private float targetFogDensity;
    private float targetAmbienceVolume;

    //[Range(0f, 1f)]
    //public float intensitySmoothing = 0.2f;

    //private EnvironmentProfile baseline;

    //private void Awake()
    //{
    //    if (map != null) baseline = map.baselineProfile;
    //}

    private void OnEnable()
    {
        if (emotionController != null)
            emotionController.OnStateUpdated += OnEmotionStateUpdated;
    }

    private void OnDisable()
    {
        if (emotionController != null)
            emotionController.OnStateUpdated -= OnEmotionStateUpdated;
    }

    //private void OnEnable()
    //{
    //    if (agent != null)
    //    {
    //        agent.OnEmotionParsed += OnEmotionParsed;
    //    }
    //}

    //private void OnDisable()
    //{
    //    if (agent != null)
    //    {
    //        agent.OnEmotionParsed -= OnEmotionParsed;
    //    }
    //}

    private void Start()
    {
        currentLightColor = baselineLightColor;
        currentLightIntensity = baselineLightIntensity;
        currentFogColor = baselineFogColor;
        currentFogDensity = baselineFogDensity;
        currentAmbienceVolume = baselineAmbienceVolume;

        targetLightColor = currentLightColor;
        targetLightIntensity = currentLightIntensity;
        targetFogColor = currentFogColor;
        targetFogDensity = currentFogDensity;
        targetAmbienceVolume = currentAmbienceVolume;

        RenderSettings.fog = true;
        ApplyCurrentState();

        //if (map != null) baseline = map.baselineProfile;

        //if (baseline != null)
        //{
        //    SetCurrentAndTargetFromProfile(baseline);
        //    ApplyCurrentState();
        //    RenderSettings.fog = baseline.fogEnabled;
        //    TryTransitionAudio(baseline, 0f);
        //    TrySetAmbienceVolume(baseline);
        //}
    }

    private void Update()
    {
        if (experiment != null && !experiment.IsAdaptive)
        {
            targetLightColor = baselineLightColor;
            targetLightIntensity = baselineLightIntensity;
            targetFogColor = baselineFogColor;
            targetFogDensity = baselineFogDensity;
            targetAmbienceVolume = baselineAmbienceVolume;
        }

        //if (baseline == null) return;

        //if (experiment != null && !experiment.IsAdaptive)
        //{
        //    SetTargetFromProfile(baseline);
        //    RenderSettings.fog = baseline.fogEnabled;
        //}

        float t = transitionSeconds <= 0.01f
            ? 1f
            : 1f - Mathf.Exp(-Time.deltaTime / transitionSeconds);

        currentLightColor = Color.Lerp(currentLightColor, targetLightColor, t);
        currentLightIntensity = Mathf.Lerp(currentLightIntensity, targetLightIntensity, t);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, t);
        currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, t);
        currentAmbienceVolume = Mathf.Lerp(currentAmbienceVolume, targetAmbienceVolume, t);

        ApplyCurrentState();
    }

    private void OnEmotionStateUpdated(DimensionalEmotionData state)
    {
        float negative = Mathf.Clamp01(-state.valence); // more negative = larger
        float tension = Mathf.Clamp01((state.arousal + state.stress) * 0.5f);
        float helplessness = 1f - state.dominance;

        // Base mood colors
        Color neutralColor = new Color(1.0f, 0.97f, 0.92f);   // warm white
        Color anxiousColor = new Color(0.60f, 0.78f, 1.00f);  // blue
        Color angryColor = new Color(1.00f, 0.35f, 0.25f);    // red/orange
        Color resignedColor = new Color(0.55f, 0.62f, 0.78f); // blue-grey

        // Compute how much each emotional region applies
        float angryWeight = Mathf.Clamp01(state.arousal * state.dominance * (-state.valence));
        float anxiousWeight = Mathf.Clamp01(state.stress * state.arousal * (1f - state.dominance));
        float resignedWeight = Mathf.Clamp01((1f - state.arousal) * helplessness * (-state.valence));
        float calmWeight = Mathf.Clamp01(1f - Mathf.Max(angryWeight, Mathf.Max(anxiousWeight, resignedWeight)));

        // Blend colors by weighted contribution
        Color weightedColor =
            (neutralColor * calmWeight) +
            (anxiousColor * anxiousWeight) +
            (angryColor * angryWeight) +
            (resignedColor * resignedWeight);

        // Normalize so the color doesn't get too dark
        float totalWeight = calmWeight + anxiousWeight + angryWeight + resignedWeight;
        if (totalWeight > 0.001f)
            weightedColor /= totalWeight;

        targetLightColor = weightedColor;

        targetLightIntensity = Mathf.Lerp(
            1.8f,   // stronger when more dominant / active
            0.7f,   // dimmer when more helpless / stressed
            Mathf.Clamp01(helplessness * 0.6f + tension * 0.4f)
        );

        targetFogColor = Color.Lerp(
            new Color(0.65f, 0.65f, 0.68f),
            new Color(0.42f, 0.46f, 0.55f),
            tension
        );

        targetFogDensity = Mathf.Lerp(
            0.003f,
            0.05f,
            Mathf.Clamp01(state.stress * 0.75f + helplessness * 0.25f)
        );

        targetAmbienceVolume = Mathf.Clamp01(0.6f + state.stress * 0.3f + state.arousal * 0.1f);
    }

    // Make environment changes based on agent emotion response
    //private void OnEmotionParsed(EmotionParser.EmotionData emo)
    //{
    //    if (map == null || baseline == null) return;

    //    Debug.Log(emo.emotion + " " + emo.intensity);

    //    emo.emotion = ClampEmotion(emo.emotion);

    //    EnvironmentProfile emotionProfile = map.GetProfile(emo.emotion);
    //    if (emotionProfile == null)
    //        emotionProfile = baseline;

    //    float raw = Mathf.Clamp01(emo.intensity);
    //    float smoothedIntensity = Mathf.Lerp(0f, raw, 1f - intensitySmoothing);

    //    SetTargetFromBlend(baseline, emotionProfile, smoothedIntensity);
    //}

    // Fallback in case of invalid emotion response
    //private string ClampEmotion(string emotion)
    //{
    //    switch (emotion.ToLowerInvariant())
    //    {
    //        case "anxious":
    //        case "angry":
    //        case "sad":
    //        case "happy":
    //            return emotion.ToLowerInvariant();
    //        default:
    //            return "anxious";
    //    }
    //}

    //private void SetCurrentAndTargetFromProfile(EnvironmentProfile p)
    //{
    //    currentLightColor = p.mainLightColor;
    //    currentLightIntensity = p.mainLightIntensity;
    //    currentFogColor = p.fogColor;
    //    currentFogDensity = p.fogDensity;
    //    currentAmbienceVolume = p.ambienceVolume;

    //    targetLightColor = p.mainLightColor;
    //    targetLightIntensity = p.mainLightIntensity;
    //    targetFogColor = p.fogColor;
    //    targetFogDensity = p.fogDensity;
    //    targetAmbienceVolume = p.ambienceVolume;
    //}

    //private void SetTargetFromProfile(EnvironmentProfile p)
    //{
    //    if (p == null) return;

    //    targetLightColor = p.mainLightColor;
    //    targetLightIntensity = p.mainLightIntensity;
    //    targetFogColor = p.fogColor;
    //    targetFogDensity = p.fogDensity;
    //    targetAmbienceVolume = p.ambienceVolume;
    //}

    //private void SetTargetFromBlend(EnvironmentProfile a, EnvironmentProfile b, float t)
    //{
    //    if (a == null || b == null) return;

    //    targetLightColor = Color.Lerp(a.mainLightColor, b.mainLightColor, t);
    //    targetLightIntensity = Mathf.Lerp(a.mainLightIntensity, b.mainLightIntensity, t);
    //    targetFogColor = Color.Lerp(a.fogColor, b.fogColor, t);
    //    targetFogDensity = Mathf.Lerp(a.fogDensity, b.fogDensity, t);
    //    targetAmbienceVolume = Mathf.Lerp(a.ambienceVolume, b.ambienceVolume, t);
    //}

    private void ApplyCurrentState()
    {
        if (mainLights != null)
        {
            foreach (Light lightUnit in mainLights)
            {
                if (lightUnit == null) continue;

                lightUnit.color = currentLightColor;
                lightUnit.intensity = currentLightIntensity;
            }
        }

        RenderSettings.fog = true;
        RenderSettings.fogColor = currentFogColor;
        RenderSettings.fogDensity = currentFogDensity;

        if (!string.IsNullOrEmpty(ambienceVolumeParameter) && audioMixer != null)
        {
            float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(currentAmbienceVolume));
            audioMixer.SetFloat(ambienceVolumeParameter, db);
        }
    }

    //private void TryTransitionAudio(EnvironmentProfile profile, float seconds)
    //{
    //    if (audioMixer == null || profile == null) return;
    //    if (string.IsNullOrEmpty(profile.audioSnapshotName)) return;

    //    var snapshot = audioMixer.FindSnapshot(profile.audioSnapshotName);
    //    if (snapshot != null)
    //    {
    //        snapshot.TransitionTo(seconds);
    //    }
    //}

    //private void TrySetAmbienceVolume(EnvironmentProfile profile)
    //{
    //    if (audioMixer == null || profile == null) return;
    //    if (string.IsNullOrEmpty(ambienceVolumeParameter)) return;

    //    float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(profile.ambienceVolume));
    //    audioMixer.SetFloat(ambienceVolumeParameter, db);
    //}
}