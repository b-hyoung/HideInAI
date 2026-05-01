using UnityEngine;

/// <summary>
/// 에너미 배회 AI. Idle(멈춤/두리번) <-> Walk(이동) 상태를 반복.
/// NavMesh 없이 transform 직접 이동.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float turnSpeed = 100f;
    [SerializeField] private float wanderRadius = 7f;
    [SerializeField] private float arriveDistance = 0.35f;

    [Header("대기")]
    [SerializeField] private float idleMinTime = 1.5f;
    [SerializeField] private float idleMaxTime = 4.5f;
    [SerializeField] private float lookAroundSpeed = 30f; // 두리번 속도 (deg/s)

    [Header("걸음 연출")]
    [SerializeField] private float headBobAmount = 0.04f;
    [SerializeField] private float headBobFrequency = 4f;

    private enum State { Idle, Walking }
    private State state;

    private Vector3 homePosition;
    private Vector3 targetPosition;
    private float idleTimer;
    private float lookSign = 1f;       // 두리번 방향
    private float lookSwitchTimer;

    private Transform headTransform;
    private Vector3 headBaseLocalPos;
    private float bobPhase;

    private void Start()
    {
        homePosition = transform.position;
        headTransform = transform.Find("Head");
        if (headTransform != null) headBaseLocalPos = headTransform.localPosition;

        EnterIdle();
    }

    private void Update()
    {
        if (state == State.Idle)
            UpdateIdle();
        else
            UpdateWalking();
    }

    // ── Idle ──────────────────────────────────────────────────────────────────

    private void EnterIdle()
    {
        state = State.Idle;
        idleTimer = Random.Range(idleMinTime, idleMaxTime);
        lookSign = Random.value > 0.5f ? 1f : -1f;
        lookSwitchTimer = Random.Range(0.8f, 2f);
    }

    private void UpdateIdle()
    {
        // 두리번거리기
        lookSwitchTimer -= Time.deltaTime;
        if (lookSwitchTimer <= 0f)
        {
            lookSign = -lookSign;
            lookSwitchTimer = Random.Range(0.8f, 2f);
        }
        transform.Rotate(0, lookSign * lookAroundSpeed * Time.deltaTime, 0);

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
            EnterWalk();
    }

    // ── Walking ───────────────────────────────────────────────────────────────

    private void EnterWalk()
    {
        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        targetPosition = homePosition + new Vector3(rand.x, 0, rand.y);
        state = State.Walking;
        bobPhase = 0f;
    }

    private void UpdateWalking()
    {
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0;

        if (toTarget.magnitude < arriveDistance)
        {
            StopBob();
            EnterIdle();
            return;
        }

        // 방향 회전
        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        // 앞으로 이동
        float actualSpeed = moveSpeed * Mathf.Min(1f, toTarget.magnitude); // 목적지 가까워지면 감속
        transform.position += transform.forward * actualSpeed * Time.deltaTime;

        // 머리 상하 흔들림
        ApplyHeadBob();
    }

    // ── 머리 흔들림 ────────────────────────────────────────────────────────────

    private void ApplyHeadBob()
    {
        if (headTransform == null) return;

        bobPhase += headBobFrequency * Time.deltaTime;
        float bob = Mathf.Sin(bobPhase * Mathf.PI * 2f) * headBobAmount;
        headTransform.localPosition = headBaseLocalPos + new Vector3(0, bob, 0);
    }

    private void StopBob()
    {
        if (headTransform == null) return;
        headTransform.localPosition = headBaseLocalPos;
    }
}
