using UnityEngine;

public class HeartbeatLoopEvent : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;

    [Header("Playback")]
    public float minVolume = 0.10f;
    public float maxVolume = 0.35f;
    public float minPitch = 0.90f;
    public float maxPitch = 1.12f;

    public void UpdateLoopState(DimensionalEmotionData state)
    {
        if (audioSource == null)
            return;

        float negative = Mathf.Clamp01((-state.valence + 1f) * 0.5f);
        float activated = Mathf.Clamp01((state.arousal + 1f) * 0.5f);
        float submissive = Mathf.Clamp01((-state.dominance + 1f) * 0.5f);

        float fearLike = negative * activated * submissive;
        bool shouldPlay = fearLike > 0.45f;

        if (shouldPlay)
        {
            audioSource.volume = Mathf.Lerp(minVolume, maxVolume, fearLike);
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, fearLike);

            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}