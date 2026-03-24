using System;
using UnityEngine;

[Serializable]
public class DimensionalEmotionData
{
    [Range(-1f, 1f)] public float valence = -0.4f;
    [Range(0f, 1f)] public float arousal = 0.5f;
    [Range(0f, 1f)] public float dominance = 0.3f;
    [Range(0f, 1f)] public float stress = 0.6f;

    public DimensionalEmotionData Clone()
    {
        return new DimensionalEmotionData
        {
            valence = valence,
            arousal = arousal,
            dominance = dominance,
            stress = stress
        };
    }
}