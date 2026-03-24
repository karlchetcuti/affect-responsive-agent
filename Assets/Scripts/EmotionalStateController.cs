using UnityEngine;

public class EmotionalStateController : MonoBehaviour
{
    [Header("State")]
    public DimensionalEmotionData baselineState = DimensionalEmotionData.Neutral();
    public DimensionalEmotionData currentState;
    public DimensionalEmotionData targetState;
    public string emotionLabel;

    private bool inDebug = false;

    [Header("Dynamics")]
    [Tooltip("Set from pilot calibration, not theory by fiat. 0 = immediate.")]
    [Min(0f)] public float responseHalfLifeSeconds = 0f;

    [Tooltip("Set from pilot calibration, not theory by fiat. 0 = no autonomous return.")]
    [Min(0f)] public float recoveryHalfLifeSeconds = 0f;

    public System.Action<DimensionalEmotionData> OnStateUpdated;

    private static readonly DimensionalEmotionData Joy = new DimensionalEmotionData
    {
        valence = 0.76f,
        arousal = 0.48f,
        dominance = 0.35f
    };

    private static readonly DimensionalEmotionData Anger = new DimensionalEmotionData
    {
        valence = -0.43f,
        arousal = 0.67f,
        dominance = 0.34f
    };

    private static readonly DimensionalEmotionData Fear = new DimensionalEmotionData
    {
        valence = -0.64f,
        arousal = 0.60f,
        dominance = -0.43f
    };

    private static readonly DimensionalEmotionData Sadness = new DimensionalEmotionData
    {
        valence = -0.63f,
        arousal = -0.27f,
        dominance = -0.33f
    };

    private static readonly DimensionalEmotionData Surprise = new DimensionalEmotionData
    {
        valence = 0.40f,
        arousal = 0.67f,
        dominance = -0.13f
    };

    private static readonly DimensionalEmotionData Disgust = new DimensionalEmotionData
    {
        valence = -0.60f,
        arousal = 0.35f,
        dominance = 0.11f
    };

    private void Start()
    {
        ResetToBaseline();
    }

    private void Update()
    {
        float responseK = HalfLifeToLerp(responseHalfLifeSeconds, Time.deltaTime);

        currentState.valence = Mathf.Lerp(currentState.valence, targetState.valence, responseK);
        currentState.arousal = Mathf.Lerp(currentState.arousal, targetState.arousal, responseK);
        currentState.dominance = Mathf.Lerp(currentState.dominance, targetState.dominance, responseK);

        OnStateUpdated?.Invoke(currentState);
    }

    public void ResetToBaseline()
    {
        currentState = baselineState.Clone();
        targetState = baselineState.Clone();
    }

    public void ApplyLLMEmotion(DimensionalEmotionData llmEmotion)
    {
        targetState.valence = Mathf.Clamp(llmEmotion.valence, -1f, 1f);
        targetState.arousal = Mathf.Clamp(llmEmotion.arousal, -1f, 1f);
        targetState.dominance = Mathf.Clamp(llmEmotion.dominance, -1f, 1f);

        inDebug = true;
    }

    public string GetEmotionLabel()
    {
        string bestLabel = "neutral";
        float bestDistance = Distance(currentState, DimensionalEmotionData.Neutral());

        TryPrototype("joy", Joy, ref bestLabel, ref bestDistance);
        TryPrototype("anger", Anger, ref bestLabel, ref bestDistance);
        TryPrototype("fear/anxiety", Fear, ref bestLabel, ref bestDistance);
        TryPrototype("sadness", Sadness, ref bestLabel, ref bestDistance);
        TryPrototype("surprise", Surprise, ref bestLabel, ref bestDistance);
        TryPrototype("disgust", Disgust, ref bestLabel, ref bestDistance);

        emotionLabel = bestLabel;

        return bestLabel;
    }

    private void TryPrototype(
        string label,
        DimensionalEmotionData proto,
        ref string bestLabel,
        ref float bestDistance)
    {
        float d = Distance(currentState, proto);
        if (d < bestDistance)
        {
            bestDistance = d;
            bestLabel = label;
        }
    }

    private static float Distance(DimensionalEmotionData a, DimensionalEmotionData b)
    {
        float dv = a.valence - b.valence;
        float da = a.arousal - b.arousal;
        float dd = a.dominance - b.dominance;
        return Mathf.Sqrt(dv * dv + da * da + dd * dd);
    }

    private static float HalfLifeToLerp(float halfLifeSeconds, float dt)
    {
        if (halfLifeSeconds <= 0f)
            return 1f;

        return 1f - Mathf.Pow(0.5f, dt / halfLifeSeconds);
    }
}