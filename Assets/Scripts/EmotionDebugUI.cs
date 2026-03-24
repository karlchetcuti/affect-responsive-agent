using TMPro;
using UnityEngine;

public class EmotionDebugUI : MonoBehaviour
{
    public EmotionalStateController emotionController;
    public TextMeshProUGUI debugText;

    private void Update()
    {
        if (emotionController == null || debugText == null)
            return;

        var s = emotionController.currentState;
        debugText.text =
            $"State: {emotionController.GetDebugLabel()}\n" +
            $"Pleasure/Valence: {s.valence:F2}\n" +
            $"Arousal: {s.arousal:F2}\n" +
            $"Dominance: {s.dominance:F2}";
    }
}