using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// XRI Action 시스템을 우회하고 InputDevices에서 직접 스틱 입력 읽어서 XR Origin 이동.
/// Dynamic Move Provider가 잘못된 입력 받을 때 대체용.
/// 사용법: Locomotion > Move 안에 부착, Target에 XR Origin 드래그, Forward Source에 Main Camera 드래그
/// (Target 비워두면 부모 체인에서 자동으로 최상위(XR Origin) 찾음)
/// </summary>
public class DirectXRMover : MonoBehaviour
{
    [SerializeField] private Transform target; // 이동시킬 대상 (보통 XR Origin)
    [SerializeField] private Transform forwardSource;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float deadZone = 0.15f;

    private InputDevice leftDevice;
    private List<InputDevice> deviceList = new List<InputDevice>();

    private void Awake()
    {
        if (target == null)
        {
            // 부모 체인 따라가서 최상위(XR Origin) 자동 탐색
            Transform t = transform;
            while (t.parent != null) t = t.parent;
            target = t;
        }
    }

    private void Update()
    {
        if (!leftDevice.isValid)
        {
            FindLeftController();
            if (!leftDevice.isValid) return;
        }

        if (!leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 stick))
            return;

        if (stick.magnitude < deadZone) return;

        Vector3 forward = forwardSource != null ? forwardSource.forward : Vector3.forward;
        Vector3 right = forwardSource != null ? forwardSource.right : Vector3.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 moveDir = forward * stick.y + right * stick.x;
        target.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void FindLeftController()
    {
        InputDeviceCharacteristics characteristics =
            InputDeviceCharacteristics.Controller |
            InputDeviceCharacteristics.Left;

        InputDevices.GetDevicesWithCharacteristics(characteristics, deviceList);
        if (deviceList.Count > 0) leftDevice = deviceList[0];
    }
}
