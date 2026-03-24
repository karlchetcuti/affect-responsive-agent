using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaStreamHandler : DownloadHandlerScript
{
    [Serializable]
    private class GenerateChunk
    {
        public string response;
        public bool done;
        public string error;
    }

    private readonly StringBuilder buffer = new StringBuilder();
    private readonly Action<string> onToken;
    private readonly Action onDone;
    private readonly Action<string> onError;

    public OllamaStreamHandler(Action<string> onToken, Action onDone = null, Action<string> onError = null)
    {
        this.onToken = onToken;
        this.onDone = onDone;
        this.onError = onError;
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength <= 0) return false;

        buffer.Append(Encoding.UTF8.GetString(data, 0, dataLength));

        while (true)
        {
            string current = buffer.ToString();
            int newlineIndex = current.IndexOf('\n');
            if (newlineIndex < 0) break;

            string line = current.Substring(0, newlineIndex).Trim();
            buffer.Remove(0, newlineIndex + 1);

            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var chunk = JsonUtility.FromJson<GenerateChunk>(line);

                if (!string.IsNullOrEmpty(chunk.error))
                {
                    onError?.Invoke(chunk.error);
                    continue;
                }

                if (!string.IsNullOrEmpty(chunk.response))
                    onToken?.Invoke(chunk.response);

                if (chunk.done)
                    onDone?.Invoke();
            }
            catch (Exception e)
            {
                onError?.Invoke($"JSON parse failed: {e.Message}\nLine: {line}");
            }
        }

        return true;
    }
}