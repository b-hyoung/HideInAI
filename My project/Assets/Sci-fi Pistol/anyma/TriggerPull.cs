using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]

public class TriggerPull : MonoBehaviour
{

    [Header("트리거 당김 설정")]
    [SerializeField] private InputActionAsset controller;
    [SerializeField] private Quaternion restRotation; // 트리거의 원래 회전값
    

    // Start is called before the first frame update
    void Start()
    {
        restRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        float RT = controller.actionMaps[5].actions[3].ReadValue<float>();

        // 트리거 당겨진 각도
        float triggerAngle = RT * 30f; // 예시로 최대 30도까지 당겨지는 것으로 설정
        // 트리거의 회전 적용
        transform.localRotation = restRotation * Quaternion.Euler(-triggerAngle, 0, 0);
    }
}
