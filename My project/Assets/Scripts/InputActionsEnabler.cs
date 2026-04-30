using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// XRI Default Input Actions의 모든 액션 맵을 강제로 활성화.
/// Input Action Manager가 없거나 작동 안 할 때 임시 해결책.
/// </summary>
public class InputActionsEnabler : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            Debug.LogWarning("[InputActionsEnabler] inputActions 필드가 비어있습니다. XRI Default Input Actions 드래그해주세요.");
            return;
        }

        foreach (var actionMap in inputActions.actionMaps)
        {
            actionMap.Enable();
            Debug.Log($"[InputActionsEnabler] 활성화: {actionMap.name}");
        }
    }

    private void OnDisable()
    {
        if (inputActions == null) return;

        foreach (var actionMap in inputActions.actionMaps)
        {
            actionMap.Disable();
        }
    }
}
