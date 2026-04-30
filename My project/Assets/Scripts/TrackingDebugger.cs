using UnityEngine;

/// <summary>
/// 컨트롤러/카메라 위치를 1초마다 로그로 출력. Right Controller에 직접 부착해서 확인용.
/// </summary>
public class TrackingDebugger : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    private float timer;

    private void Start()
    {
        if (mainCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null) mainCamera = cam.transform;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1f) return;
        timer = 0f;

        string log = "[Tracking] ";
        if (mainCamera != null)
            log += $"Cam:({mainCamera.position.x:F2},{mainCamera.position.y:F2},{mainCamera.position.z:F2}) ";
        if (leftController != null)
            log += $"L:({leftController.position.x:F2},{leftController.position.y:F2},{leftController.position.z:F2}) ";
        if (rightController != null)
            log += $"R:({rightController.position.x:F2},{rightController.position.y:F2},{rightController.position.z:F2})";

        Debug.Log(log);
    }
}
