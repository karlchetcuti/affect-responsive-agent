using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Dissertation/Emotion To Environment Map", fileName = "EmotionToEnvironmentMap")]
public class EmotionToEnvironmentMap : ScriptableObject
{
    [Serializable]
    public class Mapping
    {
        public string emotion;
        public EnvironmentProfile profile;
    }

    public EnvironmentProfile baselineProfile;
    public Mapping[] mappings;

    public EnvironmentProfile GetProfile(string emotion)
    {
        if (string.IsNullOrEmpty(emotion))
            return baselineProfile;

        string key = emotion.Trim().ToLowerInvariant();

        foreach (var m in mappings)
        {
            if (m != null && !string.IsNullOrEmpty(m.emotion) &&
                m.emotion.Trim().ToLowerInvariant() == key &&
                m.profile != null)
            {
                return m.profile;
            }
        }

        return baselineProfile;
    }
}