using System.Collections;
using UnityEngine;

public class FolderSlideEvent : MonoBehaviour
{
    [Header("References")]
    public Transform folderObject;

    [Header("Trigger")]
    [Range(0f, 1f)] public float baseProbability = 0.15f;
    [Range(0f, 1f)] public float intensityBonus = 0.35f;

    [Header("Motion")]
    public Vector3 localSlideOffset = new Vector3(0.08f, 0f, 0.02f);
    public float moveDuration = 1.5f;
    public bool returnToStart = false;

    private bool isRunning;
    private Vector3 startLocalPosition;

    private void Awake()
    {
        if (folderObject != null)
            startLocalPosition = folderObject.localPosition;
    }

    public bool TryTrigger(float intensity)
    {
        if (isRunning || folderObject == null)
            return false;

        float chance = Mathf.Clamp01(baseProbability + intensity * intensityBonus);

        Debug.Log("Folder Slide Chance:" + chance);

        if (Random.value > chance)
            return false;

        StartCoroutine(SlideRoutine());
        return true;
    }

    private IEnumerator SlideRoutine()
    {
        Debug.Log("Executed Folder Slide Event");

        isRunning = true;

        Vector3 from = folderObject.localPosition;
        Vector3 to = startLocalPosition + localSlideOffset;

        float time = 0f;
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            folderObject.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }

        folderObject.localPosition = to;

        if (returnToStart)
        {
            yield return new WaitForSeconds(1f);

            time = 0f;
            Vector3 backFrom = folderObject.localPosition;
            while (time < moveDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / moveDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                folderObject.localPosition = Vector3.Lerp(backFrom, startLocalPosition, t);
                yield return null;
            }

            folderObject.localPosition = startLocalPosition;
        }

        isRunning = false;
    }
}