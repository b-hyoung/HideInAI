using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class TriggerPull : MonoBehaviour
{
    [Header("트리거 당김 설정")]
    [SerializeField] private Vector3 pullEuler = new Vector3(-15f, 0f, 0f);
    [SerializeField] private float pullTime = 0.03f;
    [SerializeField] private float returnTime = 0.08f;

    private Quaternion restLocalRot;
    private bool captured;
    private Coroutine current;

    private void Start()
    {
        restLocalRot = transform.localRotation;
        captured = true;
    }

    public void Pull()
    {
        if (!captured)
        {
            restLocalRot = transform.localRotation;
            captured = true;
        }
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(PullRoutine());
    }

    private IEnumerator PullRoutine()
    {
        Quaternion pulled = restLocalRot * Quaternion.Euler(pullEuler);
        Quaternion startRot = transform.localRotation;

        float t = 0f;
        while (t < pullTime)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRot, pulled, t / pullTime);
            yield return null;
        }
        transform.localRotation = pulled;

        t = 0f;
        while (t < returnTime)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / returnTime);
            transform.localRotation = Quaternion.Slerp(pulled, restLocalRot, p);
            yield return null;
        }
        transform.localRotation = restLocalRot;
        current = null;
    }
}
