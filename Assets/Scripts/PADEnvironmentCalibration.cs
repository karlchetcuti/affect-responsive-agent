using UnityEngine;

[CreateAssetMenu(fileName = "PADEnvironmentCalibration", menuName = "Scriptable Objects/PADEnvironmentCalibration")]
public class PADEnvironmentCalibration : ScriptableObject
{
    [Header("Pleasure/Valence")]
    public Gradient valenceGradient;
    public AnimationCurve valenceToLightIntensity;
    public AnimationCurve valenceToFogDensity;

    [Header("Arousal")]
    public AnimationCurve arousalToLightJitterAmount;

    [Header("Dominance")]
    public AnimationCurve dominanceToLightIntensityOffset;
    public AnimationCurve dominanceToFogDensityOffset;
}