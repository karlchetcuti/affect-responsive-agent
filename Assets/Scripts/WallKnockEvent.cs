using UnityEngine;

public class WallKnockEvent : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;
    public AudioClip[] clips;

    [Header("Trigger")]
    [Range(0f, 1f)] public float baseProbability = 0.03f;
    [Range(0f, 1f)] public float intensityBonus = 0.08f;

    [Header("Playback")]
    public float minVolume = 0.35f;
    public float maxVolume = 0.65f;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    [Header("Cooldown")]
    public float cooldownSeconds = 15f;

    private float nextAllowedTime = 0f;

    public bool TryTrigger(float intensity)
    {
        if (Time.time < nextAllowedTime)
            return false;

        if (audioSource == null || clips == null || clips.Length == 0)
            return false;

        float chance = Mathf.Clamp01(baseProbability + intensity * intensityBonus);
        if (Random.value > chance)
            return false;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, intensity);
        audioSource.PlayOneShot(clip);

        nextAllowedTime = Time.time + cooldownSeconds;

        Debug.Log("Executed Wall Knock Event");

        return true;
    }
}