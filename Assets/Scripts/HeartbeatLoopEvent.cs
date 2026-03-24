using UnityEngine;

public class HeartbeatLoopEvent : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;

    [Header("Thresholds")]
    public float minStress = 0.88f;
    public float minArousal = 0.78f;
    public float maxDominance = 0.40f;

    [Header("Playback")]
    public float minVolume = 0.10f;
    public float maxVolume = 0.35f;
    public float minPitch = 0.90f;
    public float maxPitch = 1.12f;

    public void UpdateLoopState(DimensionalEmotionData state)
    {
        if (audioSource == null)
            return;

        bool shouldPlay =
            state.stress >= minStress &&
            state.arousal >= minArousal &&
            state.dominance <= maxDominance;

        if (shouldPlay)
        {
            float intensity = Mathf.Clamp01((state.stress + state.arousal) * 0.5f);

            audioSource.volume = Mathf.Lerp(minVolume, maxVolume, intensity);
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, intensity);

            if (!audioSource.isPlaying)
            {
                Debug.Log("Executed Heartbeat Loop Event");
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                Debug.Log("Stopped Heartbeat Loop Event");
                audioSource.Stop();
            }
        }
    }
}