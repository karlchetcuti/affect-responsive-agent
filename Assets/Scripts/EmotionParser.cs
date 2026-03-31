using System;
using UnityEngine;

public static class EmotionParser
{
    [Serializable]
    public class EmotionData
    {
        public string emotion;
        public float intensity;
    }

    public static bool TryExtract(string text, out EmotionData data, out string cleanedText)
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
            data = JsonUtility.FromJson<EmotionData>(json);
            cleanedText = (text.Substring(0, lastBrace)).Trim();
            return data != null && !string.IsNullOrEmpty(data.emotion);
        }
        catch
        {
            return false;
        }
    }
}