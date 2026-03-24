using System;
using UnityEngine;

public interface ISttService
{
    bool IsListening { get; }
    void StartListening(
        Action<string> onFinalText,
        Action<string> onPartialText = null,
        Action<string> onError = null
        );
    void StopListening();
}
