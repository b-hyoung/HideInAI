using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Half-Life Alyx 스타일 수동 슬라이드 레일.
/// Slide를 자체 잡기 가능 + 레일 축으로만 움직임 + 일정 거리 당겼다 놓으면 장전.
/// 발사 시 자동 반동도 처리 (SlideRecoil 대체).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SlideRail : MonoBehaviour
{
    [Header("Gun 참조")]
    [SerializeField] private Gun gun;

    [Header("레일 제약 (로컬 축)")]
    [SerializeField] private Vector3 railDirection = new Vector3(0f, 0f, -1f);
    [SerializeField] private float maxPullDistance = 0.04f;
    [SerializeField] private float chamberThreshold = 0.025f;

    [Header("스프링 / 자동 반동")]
    [SerializeField] private float springSpeed = 0.6f;
    [SerializeField] private float autoRecoilDistance = 0.025f;
    [SerializeField] private float autoRecoilTime = 0.04f;
    [SerializeField] private float autoReturnTime = 0.1f;

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private Vector3 restLocalPos;
    private bool grabbed;
    private Transform interactorTransform;
    private Coroutine autoRecoilCoroutine;
    private bool captured;

    private void Awake()
    {
        if (gun == null) gun = GetComponentInParent<Gun>();

        grab = GetComponent<XRGrabInteractable>();
        if (grab == null) grab = gameObject.AddComponent<XRGrabInteractable>();
        grab.trackPosition = false;
        grab.trackRotation = false;
        grab.throwOnDetach = false;
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
    }

    private void Start()
    {
        restLocalPos = transform.localPosition;
        captured = true;
    }

    private void OnEnable()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrabSlide);
            grab.selectExited.AddListener(OnReleaseSlide);
        }
    }

    private void OnDisable()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrabSlide);
            grab.selectExited.RemoveListener(OnReleaseSlide);
        }
    }

    private void OnGrabSlide(SelectEnterEventArgs args)
    {
        grabbed = true;
        interactorTransform = args.interactorObject.transform;
        if (autoRecoilCoroutine != null)
        {
            StopCoroutine(autoRecoilCoroutine);
            autoRecoilCoroutine = null;
        }
    }

    private void OnReleaseSlide(SelectExitEventArgs args)
    {
        grabbed = false;
        interactorTransform = null;

        float scaleZ = transform.parent != null ? Mathf.Max(0.0001f, Mathf.Abs(transform.parent.lossyScale.z)) : 1f;
        float pulledLocal = Vector3.Distance(transform.localPosition, restLocalPos);
        float pulledWorld = pulledLocal * scaleZ;
        if (pulledWorld >= chamberThreshold && gun != null)
        {
            gun.ChamberRound();
        }
    }

    public void Recoil()
    {
        if (grabbed) return;
        if (!captured) return;
        if (autoRecoilCoroutine != null) StopCoroutine(autoRecoilCoroutine);
        autoRecoilCoroutine = StartCoroutine(AutoRecoilRoutine());
    }

    private IEnumerator AutoRecoilRoutine()
    {
        float scaleZ = transform.parent != null ? Mathf.Max(0.0001f, Mathf.Abs(transform.parent.lossyScale.z)) : 1f;
        Vector3 dir = railDirection.normalized;
        Vector3 backLocal = restLocalPos + dir * (autoRecoilDistance / scaleZ);
        Vector3 startPos = transform.localPosition;

        float t = 0f;
        while (t < autoRecoilTime)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, backLocal, t / autoRecoilTime);
            yield return null;
        }
        transform.localPosition = backLocal;

        t = 0f;
        while (t < autoReturnTime)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / autoReturnTime);
            transform.localPosition = Vector3.Lerp(backLocal, restLocalPos, p);
            yield return null;
        }
        transform.localPosition = restLocalPos;
        autoRecoilCoroutine = null;
    }

    private void LateUpdate()
    {
        if (!captured) return;

        if (grabbed && interactorTransform != null && transform.parent != null)
        {
            Vector3 controllerLocal = transform.parent.InverseTransformPoint(interactorTransform.position);
            Vector3 dir = railDirection.normalized;
            float scaleZ = Mathf.Max(0.0001f, Mathf.Abs(transform.parent.lossyScale.z));
            float maxLocal = maxPullDistance / scaleZ;
            Vector3 offset = controllerLocal - restLocalPos;
            float dist = Vector3.Dot(offset, dir);
            dist = Mathf.Clamp(dist, 0f, maxLocal);
            transform.localPosition = restLocalPos + dir * dist;
        }
        else if (autoRecoilCoroutine == null)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, restLocalPos, springSpeed * Time.deltaTime);
        }
    }
}
