using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PistolMecanic : MonoBehaviour
{

    public Animator pistol;
    public InputActionAsset controller;
    
    private AudioSource audioSource;
    public float rpm;
    public float delay;
    public int time;

    // 탄피 배출은 Regidbody를 통해 AddFoce로 힘을 가하는 방식의 구현
    [Header("탄피 배출")]
    [SerializeField] private GameObject shellPrefab;
    // 탄피 배출 위치
    [SerializeField] private Transform shellEjectPoint;
    // 탄피 배출 힘
    [SerializeField] private float shellEjectForce;
    // 탄피 배출 시간
    [SerializeField] private float shellLifetime;
    // 탄피 배출 크기
    [SerializeField] private float shellScale;
    [SerializeField] private float shellXForce;
    [SerializeField] private float shellYForce;
    
    void Start()
    {
        pistol.SetBool("trigger",false);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        var RT = controller.actionMaps[5].actions[2].ReadValue<float>();
        if(delay > 0) delay -= time*Time.deltaTime;
        if(delay < 0) delay = 0;

        if (RT >= 0.5f){
            if(delay == 0){
                pistol.SetBool("trigger", true);
                audioSource.Play();
                delay = rpm;
                EjectShell();
            }
        }else{
            pistol.SetBool("trigger", false);
        }
    }

    void EjectShell()
    {   
        shellLifetime = 3f; // 탄피가 사라지는 시간
        if (shellPrefab ==null) return;
        // 탄피 배출 위치 설정
        Transform pt = shellEjectPoint != null ? shellEjectPoint : transform;
        // 없으면 실행x
        if (pt == null) return;
        
        /*
        탄피 생성
            - Instantiate를 사용하여 shellPrefab을 pt의 위치와 회전으로 생성
            - 생성된 탄피는 Rigidbody를 통해 AddForce로 힘을 가하여 배출
        */
        GameObject shell = Instantiate(shellPrefab,pt.position,pt.rotation);
        // 탄피크기 0.N배 (shellScale 변수로 조절)
        shell.transform.localScale *= shellScale;
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if(rb == null)  rb = shell.AddComponent<Rigidbody>();
        
        // 탄피 배출 힘 x축 + y축
        Vector3 force = pt.right * shellXForce + pt.up * shellYForce; // 오른쪽과 위쪽으로 힘을 가함
        // Impulse 즉시 배출 , Force 지속 , VelocityChange(질량 무시) , Acceleration (질량 무시+지속)
        rb.AddForce(force,ForceMode.Impulse);
        // 매 발사 시마다 약간씩 떨어지는 위치 다르기위해 랜덤 부여
        rb.AddTorque(UnityEngine.Random.insideUnitSphere * 0.5f, ForceMode.Impulse);

        rb.AddTorque(UnityEngine.Random.insideUnitSphere * 0.5f, ForceMode.Impulse);

        Destroy(shell,shellLifetime);
    }

}
