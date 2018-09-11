﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OTouchPickUp : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        mask = 1 << LayerMask.NameToLayer("Interactable");
        col = GetComponent<SphereCollider>();
        myRigid = GetComponent<Rigidbody>();

        myBeam.SetActive(false);

        if (hand == Hands.LeftHand)
        {
            handTrigger = OVRInput.Axis1D.PrimaryHandTrigger;
        }
        else
        {
            handTrigger = OVRInput.Axis1D.SecondaryHandTrigger;
        }
    }

    public bool grabbing;
    public SphereCollider col;
    public Hands hand;
    OVRInput.Axis1D handTrigger;
    Rigidbody myRigid;
    GameObject grabbedObject;
    //public Transform lTouchController, rTouchController;
    public ControllerButton[] myButton;
    public GameObject myBeam;

    // Update is called once per frame

    RaycastHit hit;
    int mask;
    void FixedUpdate()
    {
        //OVRInput.Update();
        MyButtonUpdate();
        if (OVRInput.Get(handTrigger) > 0)
        {
            grabbing = true;
            //Debug.Log(OVRInput.Get( OVRInput.Axis1D.PrimaryHandTrigger) +" ; "+OVRInput.Get( OVRInput.Axis1D.SecondaryHandTrigger));
            if (grabbedObject == null)
            {
                Collider[] colObjects = Physics.OverlapSphere(transform.position, col.radius, mask);
                //Debug.Log("Amount: "+colObjects.Length);
                GrabableObject script;
                for (int i = 0; i < colObjects.Length; i++)
                {
                    script = colObjects[i].attachedRigidbody.GetComponent<GrabableObject>();
                    if (script.fixedJoint == null)
                    {
                        script.OnGrab(myRigid);
                        grabbedObject = colObjects[i].attachedRigidbody.gameObject;
                        break;
                    }
                }
            }
        }
        else
        {
            grabbing = false;
            if (grabbedObject != null)
            {
                grabbedObject.GetComponent<GrabableObject>().Release();
                grabbedObject = null;
            }
        }

        if(myButton[0].state == ButtonStates.Pressed){
            Debug.Log("Button 0 pressed on "+gameObject.name+" pressed");
            CastRayForMarkable();
        }

        if (myButton[1].state == ButtonStates.Pressed)
        {
            Debug.Log("Button 1 pressed on " + gameObject.name + " pressed");
            myBeam.SetActive(true);
        } else if (myButton[1].state == ButtonStates.LetGo){
            myBeam.SetActive(false);
        }

        /*
        if (hand == Hands.LeftHand)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.X))
            {
                Debug.Log("Button X pressed");
                CastRayForMarkable();
            }
            if (OVRInput.GetDown(OVRInput.RawButton.Y))
            {
                Debug.Log("Button Y pressed");
                CastRayForMarkable();
            }
        }

        if (hand == Hands.RightHand)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                Debug.Log("Button A pressed");
                CastRayForMarkable();
            }
            if (OVRInput.GetDown(OVRInput.RawButton.B))
            {
                Debug.Log("Button B pressed");
                CastRayForMarkable();
            }
        }*/
        
    }

    float castRadius = 0.2f;
    RaycastHit[] hits;
    public void CastRayForMarkable()
    {
        hits = Physics.SphereCastAll(transform.position, castRadius, transform.forward, 10, mask);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                MarkableObject script;
                script = hits[i].collider.attachedRigidbody.GetComponent<MarkableObject>();
                if (script != null && !script.marked)
                {
                    script.Mark();
					break;
                }
            }
        }
    }

    public void MyButtonUpdate(){
        for(int i=0; i < myButton.Length; i++){
            myButton[i].value = OVRInput.Get(myButton[i].ovrButton);
            if(myButton[i].value){
                if(!myButton[i].lastValue){
                    myButton[i].state = ButtonStates.Pressed;
                } else {
                    myButton[i].state = ButtonStates.Held;
                }
            } else {
                if(myButton[i].lastValue){
                    myButton[i].state = ButtonStates.LetGo;
                } else {
                    myButton[i].state = ButtonStates.NotPressed;
                }
            }
            myButton[i].lastValue = myButton[i].value;
        }
    }

}



public enum Hands { LeftHand, RightHand }

public enum ButtonStates {Held,Pressed,LetGo,NotPressed}

[System.Serializable]
public class ControllerButton {
    public ButtonStates state;
    public OVRInput.RawButton ovrButton;
    public bool value;
    public bool lastValue;
}