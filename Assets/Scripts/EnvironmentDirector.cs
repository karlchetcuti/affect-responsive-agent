using System;
using UnityEngine;
using UnityEngine.Audio;

public class EnvironmentDirector : MonoBehaviour
{
    [Header("References")]
    public ExperimentManager experiment;
    public EmotionalStateController emotionController;
    public PADEnvironmentCalibration calibration;

    [Header("Scene Targets")]
    public Light[] mainLights;

    [Header("Baseline")]
    public Color baselineLightColor = new Color(0.95f, 0.95f, 1f);
    public float baselineLightIntensity = 1.2f;
    public Color baselineFogColor = new Color(0.5f, 0.5f, 0.55f);
    public float baselineFogDensity = 0.005f;

    [Header("Ranges")]
    public float maxExtraFogDensity = 0.08f;
    public float maxExtraLightIntensityShift = 1.2f;

    [Header("Smoothing")]
    [Range(0.1f, 10f)]
    public float transitionSeconds = 6f;

    private Color currentLightColor;
    private float currentLightIntensity;
    private Color currentFogColor;
    private float currentFogDensity;

    private Color targetLightColor;
    private float targetLightIntensity;
    private Color targetFogColor;
    private float targetFogDensity;

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

    private void Start()
    {
        currentLightColor = baselineLightColor;
        currentLightIntensity = baselineLightIntensity;
        currentFogColor = baselineFogColor;
        currentFogDensity = baselineFogDensity;

        targetLightColor = currentLightColor;
        targetLightIntensity = currentLightIntensity;
        targetFogColor = currentFogColor;
        targetFogDensity = currentFogDensity;

        RenderSettings.fog = true;
        ApplyCurrentState();
    }

    private void Update()
    {
        if (experiment != null && !experiment.IsAdaptive)
        {
            targetLightColor = baselineLightColor;
            targetLightIntensity = baselineLightIntensity;
            targetFogColor = baselineFogColor;
            targetFogDensity = baselineFogDensity;
        }

        float t = transitionSeconds <= 0.01f
            ? 1f
            : 1f - Mathf.Exp(-Time.deltaTime / transitionSeconds);

        currentLightColor = Color.Lerp(currentLightColor, targetLightColor, t);
        currentLightIntensity = Mathf.Lerp(currentLightIntensity, targetLightIntensity, t);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, t);
        currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, t);

        currentLightIntensity = Mathf.Clamp(currentLightIntensity, 1f, 4f);
        currentFogDensity = Mathf.Clamp(currentFogDensity, 0f, 0.03f);

        ApplyCurrentState();
    }

    private void OnEmotionStateUpdated(DimensionalEmotionData state)
    {
        float v = Mathf.Clamp(state.valence, -1f, 1f);
        float a = Mathf.Clamp01(state.arousal);
        float d = Mathf.Clamp01(state.dominance);

        switch(emotionController.emotionLabel)
        {
            case "joy":
                targetLightColor = Color.yellow;
                break;
            case "anger":
                targetLightColor = Color.red;
                break;
            case "fear/anxiety":
                targetLightColor = Color.grey;
                break;
            case "sadness":
                targetLightColor = Color.blue;
                break;
            case "surprise":
                targetLightColor = new Color(1f, 0.5f, 0f);
                break;
            case "disgust":
                targetLightColor = Color.green;
                break;
            default:
                targetLightColor = Color.white;
                break;
        }

        Vector3 neutral = new Vector3(0.00f, 0.50f, 0.50f);
        Vector3 current = new Vector3(v, a, d);

        float maxDistanceFromNeutral = Mathf.Sqrt(1f * 1f + 0.5f * 0.5f + 0.5f * 0.5f);
        float emotionalIntensity = Mathf.Clamp01(Vector3.Distance(current, neutral) / maxDistanceFromNeutral);
        
        targetLightIntensity = Mathf.Lerp(1f, 4f, emotionalIntensity);

        if (emotionController.emotionLabel == "fear/anxiety" || emotionController.emotionLabel == "sadness")
        {
            float fogMax = emotionController.emotionLabel == "fear/anxiety" ? 0.03f : 0.02f;
            targetFogDensity = Mathf.Lerp(0f, fogMax, emotionalIntensity);
        }
        else
        {
            targetFogDensity = 0f;
        }

        targetFogColor = baselineFogColor;
    }

    private void ApplyCurrentState()
    {
        if (mainLights != null)
        {
            foreach (Light lightUnit in mainLights)
            {
                if (lightUnit == null) continue;

                if (!lightUnit.enabled)
                    lightUnit.enabled = true;

                lightUnit.color = currentLightColor;
                lightUnit.intensity = currentLightIntensity;
            }
        }

        RenderSettings.fog = currentFogDensity > 0.0001f;
        RenderSettings.fogColor = baselineFogColor;
        RenderSettings.fogDensity = currentFogDensity;
    }
}