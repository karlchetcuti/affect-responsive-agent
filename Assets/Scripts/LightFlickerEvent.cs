using System.Collections;
using UnityEngine;

public class LightFlickerEvent : MonoBehaviour
{
    [Header("References")]
    public Light[] targetLights;

    [Header("Trigger")]
    [Range(0f, 1f)] public float baseProbability = 0.2f;
    [Range(0f, 1f)] public float intensityBonus = 0.5f;

    [Header("Flicker")]
    public int minFlickers = 6;
    public int maxFlickers = 12;
    public float minOffTime = 0.05f;
    public float maxOffTime = 0.2f;
    public float minOnTime = 0.05f;
    public float maxOnTime = 0.15f;

    [Header("Cooldown")]
    public float cooldownSeconds = 15f;

    private float nextAllowedTime = 0f;

    private bool isRunning;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] flickerClips;

    [Range(0f, 1f)] public float minVolume = 0.2f;
    [Range(0f, 1f)] public float maxVolume = 0.6f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    public bool TryTrigger(float intensity)
    {
        if (Time.time < nextAllowedTime)
            return false;

        if (isRunning || targetLights == null || targetLights.Length == 0)
            return false;

        float chance = Mathf.Clamp01(baseProbability + intensity * intensityBonus);

        if (Random.value > chance)
            return false;

        StartCoroutine(FlickerRoutine());
        nextAllowedTime = Time.time + cooldownSeconds;
        return true;
    }

    private IEnumerator FlickerRoutine()
    {
        isRunning = true;

        float[] originalIntensities = new float[targetLights.Length];
        for (int i = 0; i < targetLights.Length; i++)
        {
            if (targetLights[i] != null)
                originalIntensities[i] = targetLights[i].intensity;
        }

        int flickerCount = Random.Range(minFlickers, maxFlickers + 1);

        for (int i = 0; i < flickerCount; i++)
        {
            for (int j = 0; j < targetLights.Length; j++)
            {
                if (targetLights[j] != null)
                    targetLights[j].intensity = originalIntensities[j] * 0.18f;
            }

            PlayFlickerSound();

            yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));

            for (int j = 0; j < targetLights.Length; j++)
            {
                if (targetLights[j] != null)
                    targetLights[j].intensity = originalIntensities[j];
            }

            yield return new WaitForSeconds(Random.Range(minOnTime, maxOnTime));
        }

        isRunning = false;

        Debug.Log("Executed Light Flicker Event");
    }

    private void PlayFlickerSound()
    {
        if (audioSource == null || flickerClips == null || flickerClips.Length == 0)
            return;

        AudioClip clip = flickerClips[Random.Range(0, flickerClips.Length)];

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        audioSource.PlayOneShot(clip);
    }
}