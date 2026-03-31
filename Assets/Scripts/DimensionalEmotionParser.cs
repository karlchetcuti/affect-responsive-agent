using System;
using UnityEngine;

public static class DimensionalEmotionParser
{
    [Serializable]
    private class ParsedEmotionWrapper
    {
        public float valence;
        public float arousal;
        public float dominance;
    }

    public static bool TryExtract(string text, out DimensionalEmotionData data, out string cleanedText)
    {
        data = null;
        cleanedText = text;

        int lastBrace = text.LastIndexOf('{');
        int lastClose = text.LastIndexOf('}');

        if (lastBrace < 0 || lastClose < 0 || lastClose <= lastBrace)
            return false;

        string json = text.Substring(lastBrace, lastClose - lastBrace + 1);

        try
        {
            ParsedEmotionWrapper parsed = JsonUtility.FromJson<ParsedEmotionWrapper>(json);

            data = new DimensionalEmotionData
            {
                valence = Mathf.Clamp(parsed.valence, -1f, 1f),
                arousal = Mathf.Clamp(parsed.arousal, -1f, 1f),
                dominance = Mathf.Clamp(parsed.dominance, -1f, 1f)
            };

            cleanedText = text.Substring(0, lastBrace).Trim();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}