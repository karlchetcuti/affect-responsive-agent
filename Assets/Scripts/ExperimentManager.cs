using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public enum Condition
    {
        Adaptive,
        Control
    }

    [Header("Study Condition")]
    public Condition condition = Condition.Adaptive;

    public bool IsAdaptive => condition == Condition.Adaptive;

    public void SetAdaptive(bool adaptive)
    {
        condition = adaptive ? Condition.Adaptive : Condition.Control;
    }
}