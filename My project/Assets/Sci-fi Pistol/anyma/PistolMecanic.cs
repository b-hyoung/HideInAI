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
    
    void Start()
    {
        pistol.SetBool("trigger",false);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        var LT = controller.actionMaps[2].actions[2].ReadValue<float>();
        if(delay > 0) delay -= time*Time.deltaTime;
        if(delay < 0) delay = 0;

        if (LT >= 0.5f){
            if(delay == 0){
                pistol.SetBool("trigger", true);
                audioSource.Play();
                delay = rpm;
            }
        }else{
            pistol.SetBool("trigger", false);
        }
    }
}
