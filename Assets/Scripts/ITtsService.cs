using System;
using System.Collections;
using UnityEngine;

public interface ITtsService
{
    bool IsSpeaking { get; }
    IEnumerator Speak(
        string text,
        Action onComplete = null,
        Action<string> onError = null
        );
    void StopSpeaking();
}
