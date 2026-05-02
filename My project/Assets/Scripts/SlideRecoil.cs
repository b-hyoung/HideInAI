using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SlideRecoil : MonoBehaviour
{
    [Header("반동 설정")]
    [SerializeField] private Vector3 recoilDirection = new Vector3(0f, 0f, -1f);
    [SerializeField] private float recoilDistance = 0.025f;
    [SerializeField] private float recoilTime = 0.04f;
    [SerializeField] private float returnTime = 0.1f;

    private Vector3 restLocalPos;
    private bool captured;
    private Coroutine current;

    private void Start()
    {
        restLocalPos = transform.localPosition;
        captured = true;
    }

    public void Recoil()
    {
        if (!captured)
        {
            restLocalPos = transform.localPosition;
            captured = true;
        }
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(RecoilRoutine());
    }

    private IEnumerator RecoilRoutine()
    {
        float scaleZ = transform.parent != null ? Mathf.Max(0.0001f, Mathf.Abs(transform.parent.lossyScale.z)) : 1f;
        Vector3 localOffset = recoilDirection.normalized * (recoilDistance / scaleZ);
        Vector3 backLocalPos = restLocalPos + localOffset;
        Vector3 startPos = transform.localPosition;

        float t = 0f;
        while (t < recoilTime)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, backLocalPos, t / recoilTime);
            yield return null;
        }
        transform.localPosition = backLocalPos;

        t = 0f;
        while (t < returnTime)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / returnTime);
            transform.localPosition = Vector3.Lerp(backLocalPos, restLocalPos, p);
            yield return null;
        }
        transform.localPosition = restLocalPos;
        current = null;
    }
}
