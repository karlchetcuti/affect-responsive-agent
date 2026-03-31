using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaClient : MonoBehaviour
{
    [Serializable]
    private class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream = true;
        public float temperature = 0.7f;
    }

    private const string OLLAMA_URL = "http://localhost:11434/api/generate";

    private UnityWebRequest activeRequest;

    public void CancelActive()
    {
        if (activeRequest != null)
        {
            activeRequest.Abort();
            activeRequest = null;
        }
    }

    public IEnumerator SendPrompt(
        string prompt,
        Action<string> onToken,
        Action onComplete = null,
        Action<string> onError = null,
        string model = "gemma2:9b")
    {
        CancelActive();

        var requestData = new OllamaRequest
        {
            model = model,
            prompt = prompt,
            stream = true
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        activeRequest = new UnityWebRequest(OLLAMA_URL, "POST");
        activeRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);

        var handler = new OllamaStreamHandler(
            onToken,
            () => onComplete?.Invoke(),
            err => onError?.Invoke(err)
        );

        activeRequest.downloadHandler = handler;
        activeRequest.SetRequestHeader("Content-Type", "application/json");

        yield return activeRequest.SendWebRequest();

        if (activeRequest.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(activeRequest.error);
        }

        activeRequest = null;
    }
}