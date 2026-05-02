using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class RecoilKick : MonoBehaviour
{
    [Header("반동 회전 설정")]
    [SerializeField] private Vector3 kickEuler = new Vector3(-12f, 0f, 0f);
    [SerializeField] private float kickTime = 0.05f;
    [SerializeField] private float returnTime = 0.18f;

    private Quaternion restLocalRot;
    private bool captured;
    private Coroutine current;

    private void Start()
    {
        restLocalRot = transform.localRotation;
        captured = true;
    }

    public void Kick()
    {
        if (!captured)
        {
            restLocalRot = transform.localRotation;
            captured = true;
        }
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(KickRoutine());
    }

    private IEnumerator KickRoutine()
    {
        Quaternion kicked = restLocalRot * Quaternion.Euler(kickEuler);
        Quaternion startRot = transform.localRotation;

        float t = 0f;
        while (t < kickTime)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRot, kicked, t / kickTime);
            yield return null;
        }
        transform.localRotation = kicked;

        t = 0f;
        while (t < returnTime)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / returnTime);
            transform.localRotation = Quaternion.Slerp(kicked, restLocalRot, p);
            yield return null;
        }
        transform.localRotation = restLocalRot;
        current = null;
    }
}
