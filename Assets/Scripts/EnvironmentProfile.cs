using UnityEngine;

[CreateAssetMenu(menuName = "Dissertation/Environment Profile", fileName = "EnvironmentProfile")]
public class EnvironmentProfile : ScriptableObject
{
    [Header("Lighting")]
    public Color mainLightColor = Color.white;
    [Range(0f, 2f)] public float mainLightIntensity = 1f;

    [Header("Fog")]
    public bool fogEnabled = true;
    public Color fogColor = Color.gray;
    [Range(0f, 0.1f)] public float fogDensity = 0.01f;

    [Header("Audio (optional)")]
    [Tooltip("If set, EnvironmentDirector will crossfade to this snapshot.")]
    public string audioSnapshotName;

    [Range(0f, 1f)]
    [Tooltip("Optional: master ambience volume scalar you can map to an exposed AudioMixer parameter.")]
    public float ambienceVolume = 1f;
}