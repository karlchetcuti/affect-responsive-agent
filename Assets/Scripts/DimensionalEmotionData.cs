using System;
using UnityEngine;

[Serializable]
public class DimensionalEmotionData
{
    // In this project, valence is used as the code-level name for PAD pleasure.
    [Range(-1f, 1f)] public float valence = 0f;
    [Range(-1f, 1f)] public float arousal = 0f;
    [Range(-1f, 1f)] public float dominance = 0f;

    public DimensionalEmotionData Clone()
    {
        return new DimensionalEmotionData
        {
            valence = valence,
            arousal = arousal,
            dominance = dominance
        };
    }

    public static DimensionalEmotionData Neutral()
    {
        return new DimensionalEmotionData
        {
            valence = 0f,
            arousal = 0f,
            dominance = 0f
        };
    }
}