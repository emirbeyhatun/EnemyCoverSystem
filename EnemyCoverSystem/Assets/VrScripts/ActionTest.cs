using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;


public class ActionTest : MonoBehaviour
{
    public SteamVR_Input_Sources handType; // 1
    public SteamVR_Action_Boolean teleportAction; // 2
    public SteamVR_Action_Boolean grabAction; // 3
    public SteamVR_Action_Boolean triggerAction; // 3
    public List<AudioClip> triggerAudioClip;
    private AudioSource audioSource;
    bool rightTriggerHold = false;
    bool leftTriggerHold = false;
    System.Random rnd;
    public UnityEvent rightTriggerEvent;
    public UnityEvent leftTriggerEvent;
    public GameObject cam;
    public GameObject cameraRig;
    public LayerMask layerMask;
    public float speed = 0.7f;

    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        rnd = new System.Random();
    }
    public bool GetTeleportDown() // 1
    {
        return teleportAction.GetState(handType);
    }

    public bool GetGrab() // 2
    {
        return grabAction.GetState(handType);
    }
    public bool GetTrigger() // 2
    {
        if(triggerAction.GetState(handType) == false)
        {
            rightTriggerHold = false;
        }
        if (triggerAction.GetState(handType) == false)
        {
            leftTriggerHold = false;
        }
        return triggerAction.GetState(handType);
    }

    public void Update()
    {
        //RaycastHit rayHit;
        //if(Physics.Raycast(cameraRig.transform.position, cameraRig.transform.up * -1, out rayHit, 50, layerMask))
        //{
        //    cameraRig.transform.SetPositionAndRotation(rayHit.point, cameraRig.transform.rotation);
        //}
        //if (GetTeleportDown())
        //{

        //    if (handType == SteamVR_Input_Sources.RightHand)
        //    {
        //        cameraRig.transform.Translate(cam.transform.forward * speed * Time.deltaTime);
        //    }
        //    else if (handType == SteamVR_Input_Sources.LeftHand)
        //    {

        //    }
        //}

        if (GetGrab())
        {
            print("Grab " + handType);
        }




        if (GetTrigger())
        {
            if (!rightTriggerHold)
            {
                if (handType == SteamVR_Input_Sources.RightHand)
                {
                    rightTriggerHold = true;
                    RightHandTrigger();
                }
               
            }
            if (!leftTriggerHold)
            {
                 if (handType == SteamVR_Input_Sources.LeftHand)
                {
                    leftTriggerHold = true;
                    LeftHandTrigger();
                }
            }

           
        }

        
    }
    void RightHandTrigger()
    {
        if(rightTriggerEvent != null)
        {
            if (triggerAudioClip != null)
            {
                audioSource.clip = triggerAudioClip[rnd.Next(0, triggerAudioClip.Count)];
                audioSource.Play();
            }
            rightTriggerEvent.Invoke();
        }
        print("Right Trigger " + handType);

    }

    void LeftHandTrigger()
    {
        if (leftTriggerEvent != null)
        {
            if (triggerAudioClip != null)
            {
                //audioSource.clip = triggerAudioClip[rnd.Next(0,triggerAudioClip.Count)];
                //audioSource.Play();

            }
            leftTriggerEvent.Invoke();
        }
        print("Left Trigger " + handType);

    }

    public void Empy()
    {

    }
}
