using UnityEngine;
using Photon.Pun;

/// <summary>
/// 멀티플레이 플레이어 아바타. Photon으로 동기화됨.
/// 본인(IsMine) → 로컬 XR Origin에서 위치 읽어서 적용 + 다른 플레이어에게 송신
/// 타인(!IsMine) → 네트워크에서 받은 위치 적용
/// 사용법: Resources/PlayerAvatar.prefab 만들고 PhotonView + 이 스크립트 + CharacterModel 부착
/// </summary>
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(CharacterModel))]
public class PlayerAvatar : MonoBehaviourPun, IPunObservable
{
    private CharacterModel model;
    private Transform xrCamera;
    private Transform xrLeftHand;
    private Transform xrRightHand;

    // 네트워크 동기화 변수 (타인 시점에서 보간용)
    private Vector3 netHeadPos;
    private Quaternion netHeadRot;
    private Vector3 netLeftHandPos;
    private Quaternion netLeftHandRot;
    private Vector3 netRightHandPos;
    private Quaternion netRightHandRot;

    private void Start()
    {
        model = GetComponent<CharacterModel>();

        if (photonView.IsMine)
        {
            // 본인: 로컬 XR 트래킹 소스 찾기
            FindLocalXR();
            // 본인은 자기 머리 안 보이게
            model.HideHeadForLocalPlayer();
        }
    }

    private void FindLocalXR()
    {
        // Main Camera (헤드)
        Camera cam = Camera.main;
        if (cam != null) xrCamera = cam.transform;

        // 양손 컨트롤러 (이름으로 찾기)
        var allTransforms = FindObjectsOfType<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.name == "Left Controller" || t.name == "LeftHand Controller")
                xrLeftHand = t;
            else if (t.name == "Right Controller" || t.name == "RightHand Controller")
                xrRightHand = t;
        }

        if (xrCamera == null) Debug.LogWarning("[PlayerAvatar] Main Camera 못 찾음");
        if (xrLeftHand == null) Debug.LogWarning("[PlayerAvatar] Left Controller 못 찾음");
        if (xrRightHand == null) Debug.LogWarning("[PlayerAvatar] Right Controller 못 찾음");
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            UpdateLocalAvatar();
        }
        else
        {
            UpdateRemoteAvatar();
        }
    }

    private void UpdateLocalAvatar()
    {
        if (xrCamera == null) return;

        // 1. 루트(아바타 전체)는 발 위치에 고정. 자식 업데이트 전에 먼저 옮겨야 함.
        transform.position = new Vector3(xrCamera.position.x, 0, xrCamera.position.z);
        transform.rotation = Quaternion.identity;

        // 2. 머리: 카메라 월드 위치/회전
        if (model.head != null)
        {
            model.head.position = xrCamera.position;
            model.head.rotation = xrCamera.rotation;
        }

        // 3. 손: 컨트롤러 월드 위치/회전
        if (xrLeftHand != null && model.leftHand != null)
        {
            model.leftHand.position = xrLeftHand.position;
            model.leftHand.rotation = xrLeftHand.rotation;
        }
        if (xrRightHand != null && model.rightHand != null)
        {
            model.rightHand.position = xrRightHand.position;
            model.rightHand.rotation = xrRightHand.rotation;
        }

        // 4. 몸통은 발(루트 Y)부터 머리 아래까지 늘어나는 캡슐
        FitBodyToHeight();

        // 5. 팔: 어깨~손 사이를 동적으로 연결
        FitArms();
        UpdateVisualModel();
    }

    /// <summary>
    /// 몸통을 발끝(transform.y)부터 머리 아래까지 채우게 동적으로 위치/스케일 조정.
    /// 사용자 키에 따라 자동 적응.
    /// </summary>
    private void FitBodyToHeight()
    {
        if (model.head == null || model.body == null) return;

        float feetY = transform.position.y;
        float headBottomY = model.head.position.y - 0.18f; // 머리 sphere 반지름만큼 빼기
        float bodyHeight = Mathf.Max(0.1f, headBottomY - feetY);
        float bodyCenterY = (feetY + headBottomY) * 0.5f;

        model.body.position = new Vector3(model.head.position.x, bodyCenterY, model.head.position.z);

        // 캡슐 scale Y는 절반 높이 (Unity 기본 capsule mesh 특성)
        Vector3 bodyScale = model.body.localScale;
        bodyScale.y = bodyHeight * 0.5f;
        model.body.localScale = bodyScale;

        // 회전: 머리 Yaw만 따라감
        model.body.rotation = Quaternion.Euler(0, model.head.eulerAngles.y, 0);
    }

    /// <summary>
    /// 팔을 어깨에서 손까지 연결되게 동적으로 위치/회전/스케일 조정.
    /// </summary>
    private void FitArms()
    {
        if (model.head == null || model.body == null) return;

        // 어깨 위치 (몸통 위쪽 + 좌우 오프셋)
        Vector3 bodyForward = model.body.forward;
        Vector3 bodyRight = model.body.right;
        Vector3 shoulderBase = model.head.position + Vector3.down * 0.25f;

        Vector3 leftShoulder = shoulderBase - bodyRight * 0.18f;
        Vector3 rightShoulder = shoulderBase + bodyRight * 0.18f;

        if (model.leftArm != null && model.leftHand != null)
        {
            FitArmCapsule(model.leftArm, leftShoulder, model.leftHand.position);
        }
        if (model.rightArm != null && model.rightHand != null)
        {
            FitArmCapsule(model.rightArm, rightShoulder, model.rightHand.position);
        }
    }

    /// <summary>
    /// 캡슐을 두 점(start~end) 사이에 위치+회전+스케일.
    /// </summary>
    private void FitArmCapsule(Transform arm, Vector3 start, Vector3 end)
    {
        Vector3 mid = (start + end) * 0.5f;
        Vector3 dir = end - start;
        float len = dir.magnitude;

        arm.position = mid;
        if (dir.sqrMagnitude > 0.0001f)
        {
            // 캡슐 기본 방향이 Y+이므로 dir로 회전
            arm.rotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
        }

        Vector3 scale = arm.localScale;
        scale.y = len * 0.5f; // 캡슐 mesh가 -1~1이라 절반 곱함
        arm.localScale = scale;
    }

    private void UpdateRemoteAvatar()
    {
        // 받은 위치로 부드럽게 보간
        if (model.head != null)
        {
            model.head.position = Vector3.Lerp(model.head.position, netHeadPos, Time.deltaTime * 15f);
            model.head.rotation = Quaternion.Slerp(model.head.rotation, netHeadRot, Time.deltaTime * 15f);
        }
        if (model.leftHand != null)
        {
            model.leftHand.position = Vector3.Lerp(model.leftHand.position, netLeftHandPos, Time.deltaTime * 15f);
            model.leftHand.rotation = Quaternion.Slerp(model.leftHand.rotation, netLeftHandRot, Time.deltaTime * 15f);
        }
        if (model.rightHand != null)
        {
            model.rightHand.position = Vector3.Lerp(model.rightHand.position, netRightHandPos, Time.deltaTime * 15f);
            model.rightHand.rotation = Quaternion.Slerp(model.rightHand.rotation, netRightHandRot, Time.deltaTime * 15f);
        }

        // 몸통도 머리 따라감 + 다리까지 늘어남
        FitBodyToHeight();
        FitArms();
        UpdateVisualModel();
    }

    private void UpdateVisualModel()
    {
        if (model.head == null) return;

        model.UpdateVisualPose(transform.position, model.head.eulerAngles.y);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 본인 → 다른 플레이어에게 위치/회전 전송
            stream.SendNext(model.head != null ? model.head.position : Vector3.zero);
            stream.SendNext(model.head != null ? model.head.rotation : Quaternion.identity);
            stream.SendNext(model.leftHand != null ? model.leftHand.position : Vector3.zero);
            stream.SendNext(model.leftHand != null ? model.leftHand.rotation : Quaternion.identity);
            stream.SendNext(model.rightHand != null ? model.rightHand.position : Vector3.zero);
            stream.SendNext(model.rightHand != null ? model.rightHand.rotation : Quaternion.identity);
        }
        else
        {
            // 다른 플레이어 → 본인에게 위치/회전 수신
            netHeadPos = (Vector3)stream.ReceiveNext();
            netHeadRot = (Quaternion)stream.ReceiveNext();
            netLeftHandPos = (Vector3)stream.ReceiveNext();
            netLeftHandRot = (Quaternion)stream.ReceiveNext();
            netRightHandPos = (Vector3)stream.ReceiveNext();
            netRightHandRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
