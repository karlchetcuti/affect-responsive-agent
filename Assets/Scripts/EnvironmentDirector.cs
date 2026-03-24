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

        Vector3 neutral = new Vector3(0.00f, 0.50f, 0.50f);
        Vector3 joy = new Vector3(0.76f, 0.74f, 0.68f);
        Vector3 anger = new Vector3(-0.43f, 0.84f, 0.67f);
        Vector3 fear = new Vector3(-0.64f, 0.80f, 0.29f);
        Vector3 sadness = new Vector3(-0.63f, 0.37f, 0.34f);
        Vector3 surprise = new Vector3(0.40f, 0.84f, 0.44f);
        Vector3 disgust = new Vector3(-0.60f, 0.68f, 0.56f);

        Vector3 current = new Vector3(v, a, d);

        string stateName = "Neutral";
        float bestDistance = Vector3.Distance(current, neutral);
        Color chosenColor = Color.white;

        TryEmotion("Joy", joy, Color.yellow, current, ref stateName, ref chosenColor, ref bestDistance);
        TryEmotion("Anger", anger, Color.red, current, ref stateName, ref chosenColor, ref bestDistance);
        TryEmotion("Fear/Anxiety", fear, Color.grey, current, ref stateName, ref chosenColor, ref bestDistance);
        TryEmotion("Sadness", sadness, Color.blue, current, ref stateName, ref chosenColor, ref bestDistance);
        TryEmotion("Surprise", surprise, new Color(1f, 0.5f, 0f), current, ref stateName, ref chosenColor, ref bestDistance);
        TryEmotion("Disgust", disgust, Color.green, current, ref stateName, ref chosenColor, ref bestDistance);

        float maxDistanceFromNeutral = Mathf.Sqrt(1f * 1f + 0.5f * 0.5f + 0.5f * 0.5f);
        float emotionalIntensity = Mathf.Clamp01(Vector3.Distance(current, neutral) / maxDistanceFromNeutral);

        targetLightColor = chosenColor;
        targetLightIntensity = Mathf.Lerp(1f, 4f, emotionalIntensity);

        if (stateName == "Fear/Anxiety" || stateName == "Sadness")
        {
            float fogMax = stateName == "Fear/Anxiety" ? 0.03f : 0.02f;
            targetFogDensity = Mathf.Lerp(0f, fogMax, emotionalIntensity);
        }
        else
        {
            targetFogDensity = 0f;
        }

        targetFogColor = baselineFogColor;
    }

    private void TryEmotion(
        string candidateName,
        Vector3 candidatePrototype,
        Color candidateColor,
        Vector3 current,
        ref string bestName,
        ref Color bestColor,
        ref float bestDistance)
        {
            float distance = Vector3.Distance(current, candidatePrototype);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestName = candidateName;
                bestColor = candidateColor;
            }
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