using UnityEngine;

public class EmotionalStateController : MonoBehaviour
{
    [Header("State")]
    public DimensionalEmotionData currentState = new DimensionalEmotionData();
    public DimensionalEmotionData targetState = new DimensionalEmotionData();

    [Header("Baseline Interrogation State")]
    [Range(-1f, 1f)] public float baselineValence = -0.45f;
    [Range(0f, 1f)] public float baselineArousal = 0.55f;
    [Range(0f, 1f)] public float baselineDominance = 0.30f;
    [Range(0f, 1f)] public float baselineStress = 0.60f;

    [Header("Smoothing")]
    [Range(0.1f, 10f)] public float smoothingSpeed = 2f;

    [Header("Session Modifiers")]
    [Range(0f, 1f)] public float pressure = 0.5f;
    [Range(0f, 1f)] public float trust = 0.2f;
    [Range(0f, 1f)] public float defensiveness = 0.6f;
    [Range(0f, 1f)] public float fatigue = 0f;

    public System.Action<DimensionalEmotionData> OnStateUpdated;

    private void Start()
    {
        ResetToBaseline();
    }

    private void Update()
    {
        float t = 1f - Mathf.Exp(-Time.deltaTime * smoothingSpeed);

        currentState.valence = Mathf.Lerp(currentState.valence, targetState.valence, t);
        currentState.arousal = Mathf.Lerp(currentState.arousal, targetState.arousal, t);
        currentState.dominance = Mathf.Lerp(currentState.dominance, targetState.dominance, t);
        currentState.stress = Mathf.Lerp(currentState.stress, targetState.stress, t);

        OnStateUpdated?.Invoke(currentState);
    }

    public void ResetToBaseline()
    {
        currentState.valence = baselineValence;
        currentState.arousal = baselineArousal;
        currentState.dominance = baselineDominance;
        currentState.stress = baselineStress;

        targetState = currentState.Clone();

        pressure = 0.5f;
        trust = 0.2f;
        defensiveness = 0.6f;
        fatigue = 0f;
    }

    public void ApplyLLMEmotion(DimensionalEmotionData llmEmotion)
    {
        targetState.valence = Mathf.Clamp(llmEmotion.valence, -1f, 1f);
        targetState.arousal = Mathf.Clamp01(llmEmotion.arousal);
        targetState.dominance = Mathf.Clamp01(llmEmotion.dominance);
        targetState.stress = Mathf.Clamp01(llmEmotion.stress);

        ApplyContextBiases();
        ClampToScenarioBounds();
    }

    public void ApplyPlayerHeuristics(string playerInput)
    {
        if (string.IsNullOrWhiteSpace(playerInput))
            return;

        string text = playerInput.ToLowerInvariant();

        if (text.Contains("liar") || text.Contains("lying") || text.Contains("killed") || text.Contains("murder"))
        {
            pressure = Mathf.Clamp01(pressure + 0.12f);
            defensiveness = Mathf.Clamp01(defensiveness + 0.10f);
            targetState.arousal = Mathf.Clamp01(targetState.arousal + 0.10f);
            targetState.stress = Mathf.Clamp01(targetState.stress + 0.12f);
            targetState.valence = Mathf.Clamp(targetState.valence - 0.08f, -1f, 1f);
        }

        if (text.Contains("calm down") || text.Contains("help me understand") || text.Contains("tell me the truth"))
        {
            trust = Mathf.Clamp01(trust + 0.08f);
            defensiveness = Mathf.Clamp01(defensiveness - 0.05f);
            targetState.arousal = Mathf.Clamp01(targetState.arousal - 0.05f);
        }

        if (text.Contains("where were you") || text.Contains("what happened") || text.Contains("why"))
        {
            pressure = Mathf.Clamp01(pressure + 0.04f);
            targetState.stress = Mathf.Clamp01(targetState.stress + 0.03f);
        }
    }

    private void ApplyContextBiases()
    {
        targetState.stress = Mathf.Clamp01(targetState.stress + pressure * 0.15f + defensiveness * 0.10f);
        targetState.dominance = Mathf.Clamp01(targetState.dominance + defensiveness * 0.10f - trust * 0.05f);
        targetState.valence = Mathf.Clamp(targetState.valence - pressure * 0.10f, -1f, 1f);

        fatigue = Mathf.Clamp01(fatigue + 0.01f);
        targetState.arousal = Mathf.Clamp01(targetState.arousal - fatigue * 0.05f);
    }

    private void ClampToScenarioBounds()
    {
        targetState.valence = Mathf.Clamp(targetState.valence, -1f, 0.25f);
        targetState.arousal = Mathf.Clamp01(targetState.arousal);
        targetState.dominance = Mathf.Clamp01(targetState.dominance);
        targetState.stress = Mathf.Clamp(targetState.stress, 0.2f, 1f);
    }

    public string GetDebugLabel()
    {
        if (currentState.valence > 0.05f && currentState.stress < 0.45f)
            return "calmer/cooperative";

        if (currentState.arousal > 0.75f && currentState.dominance > 0.45f)
            return "angry/defensive";

        if (currentState.arousal < 0.35f && currentState.dominance < 0.25f)
            return "resigned/sad";

        return "anxious/stressed";
    }
}