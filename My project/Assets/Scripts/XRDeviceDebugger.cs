using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// OpenXR/XR Plug-in이 실제로 어떤 디바이스를 인식하고 트래킹 데이터를 보내는지 확인.
/// InputAction을 거치지 않고 InputDevices API에서 직접 읽음.
/// </summary>
public class XRDeviceDebugger : MonoBehaviour
{
    private float timer;
    private List<InputDevice> devices = new List<InputDevice>();

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1f) return;
        timer = 0f;

        InputDevices.GetDevices(devices);
        Debug.Log($"[XRDevices] 총 {devices.Count}개 디바이스 감지됨");

        foreach (var device in devices)
        {
            string info = $"  ▸ {device.name} | {device.characteristics}";

            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
            {
                info += $" | Pos:({pos.x:F2}, {pos.y:F2}, {pos.z:F2})";
            }
            else
            {
                info += " | Pos:N/A";
            }

            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
            {
                info += $" | Rot:OK";
            }
            else
            {
                info += " | Rot:N/A";
            }

            if (device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked))
            {
                info += $" | Tracked:{tracked}";
            }

            Debug.Log(info);
        }
    }
}
