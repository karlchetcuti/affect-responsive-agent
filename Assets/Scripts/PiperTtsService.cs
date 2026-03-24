using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class PiperTtsService: MonoBehaviour, ITtsService
{
    [Header("Piper Paths")]
    public string piperExecutablePath;
    public string modelPath;
    public string outputWavFileName = "tts_output.wav";

    [Header("Playback")]
    public AudioSource audioSource;

    public bool IsSpeaking => audioSource != null && audioSource.isPlaying;

    public IEnumerator Speak(
        string text,
        Action onComplete = null,
        Action<string> onError = null
        )
    {
        if (audioSource == null)
        {
            onError?.Invoke("No AudioSource assigned.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(piperExecutablePath) || !File.Exists(piperExecutablePath))
        {
            onError?.Invoke($"Piper executable not found: {piperExecutablePath}");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
        {
            onError?.Invoke($"Piper model not found: {modelPath}");
            yield break;
        }

        string fullOutputPath = Path.Combine(Application.persistentDataPath, outputWavFileName);

        if (File.Exists(fullOutputPath))
        {
            try
            {
                File.Delete(fullOutputPath);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Could not delete old WAV: {e.Message}");
                yield break;
            }
        }

        UnityEngine.Debug.Log($"Piper EXE: {piperExecutablePath}");
        UnityEngine.Debug.Log($"Piper model: {modelPath}");
        UnityEngine.Debug.Log($"Piper output: {fullOutputPath}");
        UnityEngine.Debug.Log($"TTS text: {text}");

        var psi = new ProcessStartInfo
        {
            FileName = piperExecutablePath,
            Arguments = $"-m \"{modelPath}\" -f \"{fullOutputPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process proc = null;
        string stdOut = "";
        string stdErr = "";

        try
        {
            proc = new Process { StartInfo = psi };
            proc.Start();

            proc.StandardInput.WriteLine(text);
            proc.StandardInput.Close();
        }
        catch (Exception e)
        {
            onError?.Invoke($"Failed to launch Piper: {e.Message}");
            yield break;
        }

        while (!proc.HasExited)
            yield return null;

        try
        {
            stdOut = proc.StandardOutput.ReadToEnd();
            stdErr = proc.StandardError.ReadToEnd();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"Could not read Piper output streams: {e.Message}");
        }

        if (!string.IsNullOrWhiteSpace(stdOut))
            UnityEngine.Debug.Log("Piper stdout:\n" + stdOut);

        if (!string.IsNullOrWhiteSpace(stdErr))
            UnityEngine.Debug.LogWarning("Piper stderr:\n" + stdErr);

        if (proc.ExitCode != 0)
        {
            onError?.Invoke($"Piper failed with exit code {proc.ExitCode}. Check Console for stderr.");
            yield break;
        }

        if (!File.Exists(fullOutputPath))
        {
            onError?.Invoke($"Piper finished but no WAV was created at: {fullOutputPath}");
            yield break;
        }

        string fileUrl = new Uri(fullOutputPath).AbsoluteUri;
        UnityEngine.Debug.Log($"Loading WAV from: {fileUrl}");

        using var req = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.WAV);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Failed to load generated WAV: {req.error}");
            yield break;
        }

        var clip = DownloadHandlerAudioClip.GetContent(req);
        audioSource.clip = clip;
        audioSource.Play();

        while (audioSource.isPlaying)
            yield return null;

        onComplete?.Invoke();
    }

    public void StopSpeaking()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
}
