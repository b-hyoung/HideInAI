using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// XRI Default Input Actions를 우회하고 InputDevices API에서 직접 컨트롤러 위치/회전 읽음.
/// TrackedPoseDriver가 작동 안 할 때 대체용.
/// 사용법: Left Controller / Right Controller에 부착, Hand 설정만 맞추면 됨.
/// </summary>
public class DirectXRTracker : MonoBehaviour
{
    public enum Hand { Left, Right }

    [SerializeField] private Hand hand = Hand.Right;

    private InputDevice device;
    private List<InputDevice> deviceList = new List<InputDevice>();
    private bool deviceFound;

    private void Update()
    {
        if (!device.isValid)
        {
            FindDevice();
            if (!device.isValid) return;
        }

        if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
        {
            transform.localPosition = pos;
        }

        if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
        {
            transform.localRotation = rot;
        }
    }

    private void FindDevice()
    {
        InputDeviceCharacteristics characteristics =
            InputDeviceCharacteristics.Controller |
            (hand == Hand.Left ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right);

        InputDevices.GetDevicesWithCharacteristics(characteristics, deviceList);

        if (deviceList.Count > 0)
        {
            device = deviceList[0];
            if (!deviceFound)
            {
                deviceFound = true;
                Debug.Log($"[DirectXRTracker] {hand} 컨트롤러 연결됨: {device.name}");
            }
        }
    }
}
