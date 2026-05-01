using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// XRI Action 시스템을 우회하고 InputDevices에서 직접 오른쪽 스틱 읽어서 XR Origin 회전.
/// Continuous Turn Provider가 작동 안 할 때 대체용.
/// 사용법: Locomotion > Turn 안에 부착, Target에 XR Origin 드래그, Pivot Transform에 Main Camera 드래그
/// (Target 비워두면 부모 체인에서 자동으로 최상위(XR Origin) 찾음)
/// </summary>
public class DirectXRTurner : MonoBehaviour
{
    [SerializeField] private Transform target; // 회전시킬 대상 (보통 XR Origin)
    [SerializeField] private float turnSpeed = 60f;
    [SerializeField] private float deadZone = 0.2f;
    [SerializeField] private Transform pivotTransform; // 회전 중심 (보통 Main Camera)

    private InputDevice rightDevice;
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
        if (!rightDevice.isValid)
        {
            FindRightController();
            if (!rightDevice.isValid) return;
        }

        if (!rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 stick))
            return;

        if (Mathf.Abs(stick.x) < deadZone) return;

        float turnAmount = stick.x * turnSpeed * Time.deltaTime;

        if (pivotTransform != null)
        {
            target.RotateAround(pivotTransform.position, Vector3.up, turnAmount);
        }
        else
        {
            target.Rotate(0, turnAmount, 0);
        }
    }

    private void FindRightController()
    {
        InputDeviceCharacteristics characteristics =
            InputDeviceCharacteristics.Controller |
            InputDeviceCharacteristics.Right;

        InputDevices.GetDevicesWithCharacteristics(characteristics, deviceList);
        if (deviceList.Count > 0) rightDevice = deviceList[0];
    }
}
