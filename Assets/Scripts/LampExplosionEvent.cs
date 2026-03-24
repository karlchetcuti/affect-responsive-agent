using System.Collections;
using UnityEngine;

public class LampExplosionEvent : MonoBehaviour
{
    [Header("References")]
    public Light[] lampLights;
    public AudioSource audioSource;
    public AudioClip shatterClip;
    public Renderer[] emissiveRenderers;

    [Header("Explosion Behaviour")]
    public float baseProbability = 0.02f;
    public float intensityBonus = 0.08f;
    public float overheatDuration = 1.5f;
    public float maxIntensityMultiplier = 3.0f;

    private bool hasExploded = false;
    private bool isRunning = false;
    private float[] originalIntensities;

    private void Awake()
    {
        if (lampLights != null)
        {
            originalIntensities = new float[lampLights.Length];
            for (int i = 0; i < lampLights.Length; i++)
            {
                if (lampLights[i] != null)
                    originalIntensities[i] = lampLights[i].intensity;
            }
        }
    }

    public bool TryTrigger(float intensity)
    {
        if (hasExploded || isRunning)
            return false;

        float chance = Mathf.Clamp01(baseProbability + intensity * intensityBonus);
        if (Random.value > chance)
            return false;

        StartCoroutine(ExplosionRoutine(intensity));
        return true;
    }

    private IEnumerator ExplosionRoutine(float intensity)
    {
        isRunning = true;

        float time = 0f;

        while (time < overheatDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / overheatDuration);
            float ramp = Mathf.SmoothStep(1f, maxIntensityMultiplier, t);

            if (lampLights != null)
            {
                for (int i = 0; i < lampLights.Length; i++)
                {
                    if (lampLights[i] != null)
                        lampLights[i].intensity = originalIntensities[i] * ramp;
                }
            }

            yield return null;
        }

        if (audioSource != null && shatterClip != null)
        {
            audioSource.volume = Mathf.Lerp(0.75f, 1.0f, intensity);
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(shatterClip);
        }

        if (lampLights != null)
        {
            for (int i = 0; i < lampLights.Length; i++)
            {
                if (lampLights[i] != null)
                    lampLights[i].enabled = false;
            }
        }

        if (emissiveRenderers != null)
        {
            foreach (Renderer r in emissiveRenderers)
            {
                if (r == null) continue;

                if (r.material.HasProperty("_EmissionColor"))
                    r.material.SetColor("_EmissionColor", Color.black);
            }
        }

        hasExploded = true;
        isRunning = false;

        Debug.Log("Executed Lamp Explosion Event");
    }

    public bool HasExploded()
    {
        return hasExploded;
    }
}