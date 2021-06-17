//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using Valve.VR.InteractionSystem;

namespace Valve.VR.Extras
{
    public class SteamVR_LaserPointer : MonoBehaviour
    {
        public SteamVR_Behaviour_Pose pose;

        //public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.__actions_default_in_InteractUI;
        public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

        public bool active = true;
        public Color color;
        public float thickness = 0.004f;
        public Color clickColor = Color.green;
        //public GameObject holder;
        public GameObject pointer;
        bool isActive = false;
        public bool addRigidBody = false;
        public Transform reference;
        public event PointerEventHandler Pointer;
        public event PointerEventHandler PointerIn;
        public event PointerEventHandler PointerOut;
        public event PointerEventHandler PointerClick;
        public event PointerEventHandler PointerHold;
        public PointerEventData data;
        //public float UIDistance = Mathf.Infinity;

        public Camera handCamera;
        public List<RaycastResult> mRayCastResults;
        public InputModule inputModule;

        //public Action onUpdate;

        Transform previousContact = null;
        bool dragging = false;

        private void Start()
        {
            Debug.Assert(handCamera != null);
            if (pose == null)
                pose = this.GetComponent<SteamVR_Behaviour_Pose>();
            if (pose == null)
                Debug.LogError("No SteamVR_Behaviour_Pose component found on this object", this);

            if (interactWithUI == null)
                Debug.LogError("No ui interaction action has been set on this component.", this);

            mRayCastResults = new List<RaycastResult>();

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pointer.GetComponent<Collider>().enabled = false;
            pointer.transform.parent = handCamera.transform;
            pointer.transform.localScale = new Vector3(thickness, 100 , thickness);
            pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            pointer.transform.localRotation = Quaternion.Euler(new Vector3(90,0,0));
            pointer.layer = 2;
            BoxCollider collider = pointer.GetComponent<BoxCollider>();
            if (addRigidBody)
            {
                if (collider)
                {
                    collider.isTrigger = true;
                }
                Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (collider)
                {
                    UnityEngine.Object.Destroy(collider);
                }
            }
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;
            data = new PointerEventData(EventSystem.current);
            inputModule = InputModule.instance;
        }

        public virtual void OnPointerIn(PointerEventArgs e)
        {
            if (PointerIn != null)
                PointerIn(this, e);
        }

        public virtual void OnPointerClick(PointerEventArgs e)
        {
            if (PointerClick != null)
                PointerClick(this, e);
        }

        public virtual void OnPointerHold(PointerEventArgs e)
        {
            if (PointerHold != null)
                PointerHold (this, e);
        }


        public virtual void OnPointerOut(PointerEventArgs e)
        {
            if (PointerOut != null)
                PointerOut(this, e);
        }

        public virtual void OnPointer(PointerEventArgs e)
        {
            if (Pointer != null)
                Pointer(this, e);
        }

        public void setActive(bool state)
        {
            active = state;
            pointer.SetActive(state);
            previousContact = null;
        }


        private void Update()
        {
            if(active)
            {
                float dist = 100f;

                

                Ray raycast = new Ray(handCamera.transform.position, handCamera.transform.forward);
                //Debug.DrawRay(handCamera.transform.position, handCamera.transform.forward*3, Color.red);
                RaycastHit hit;
                bool bHit = Physics.Raycast(raycast, out hit);

                PointerEventData data = new PointerEventData(EventSystem.current);
                data.position = new Vector2(handCamera.pixelWidth / 2, handCamera.pixelHeight / 2);

                EventSystem.current.RaycastAll(data, mRayCastResults);
                GameObject nearestHit = null;
                Vector3 nearestHitPoint = Vector3.zero;
                float lowestRaycastDist = Mathf.Infinity;
                RaycastResult closestRay = new RaycastResult();
                if(mRayCastResults.Count != 0)
                {
                    closestRay = inputModule.getNearestResult(mRayCastResults);
                    
                    nearestHit = closestRay.gameObject;
                    nearestHitPoint = closestRay.worldPosition;
                    lowestRaycastDist = closestRay.distance;
                }

                Transform hitTransform = hit.transform;

                //Debug.Log(hit.distance);
                //Debug.Log(lowestRaycastDist);
                //onUpdate?.Invoke();
                bool bUIHit = false;
                if ((bHit && lowestRaycastDist < hit.distance) || (!bHit && lowestRaycastDist < 100f))
                {
                    bHit = true;
                    bUIHit = true;
                    dist = lowestRaycastDist;
                    hit.distance = lowestRaycastDist;
                    hit.point = nearestHitPoint;
                    hitTransform = nearestHit.transform;
                }
                    



                //EventSystem.current.RaycastAll()

                if (previousContact && previousContact != hitTransform)
                {
                    PointerEventArgs args = new PointerEventArgs();
                    args.fromInputSource = pose.inputSource;
                    args.distance = 0f;
                    args.flags = 0;
                    args.target = previousContact;
                    args.hit = Vector3.zero;
                    OnPointerOut(args);
                    //if(bUIHit)
                    //    inputModule.HoverEnd(previousContact.gameObject);
                    previousContact = null;
                }
                if (bHit)
                {
                    PointerEventArgs args = new PointerEventArgs();
                    args.fromInputSource = pose.inputSource;
                    args.distance = hit.distance;
                    args.flags = 0;
                    args.target = hitTransform;
                    args.hit = hit.point;
                    OnPointer(args);
                    
                }
                if (bHit && previousContact != hitTransform)
                {
                    
                    PointerEventArgs argsIn = new PointerEventArgs();
                    argsIn.fromInputSource = pose.inputSource;
                    argsIn.distance = hit.distance;
                    argsIn.flags = 0;
                    argsIn.target = hitTransform;
                    argsIn.hit = hit.point;
                    OnPointerIn(argsIn);
                    //if (bUIHit)
                    //    inputModule.HoverBegin(hitTransform.gameObject);
                    previousContact = hitTransform;
                }
                if (!bHit)
                {
                    previousContact = null;
                }
                if (bHit && hit.distance < 100f)
                {
                    dist = hit.distance;
                }

                if (bHit && interactWithUI.GetState(pose.inputSource))
                {
                    PointerEventArgs argsClick = new PointerEventArgs();
                    argsClick.fromInputSource = pose.inputSource;
                    argsClick.distance = hit.distance;
                    argsClick.flags = 0;
                    argsClick.target = hitTransform;
                    argsClick.hit = hit.point;
                    OnPointerHold(argsClick);
                    //if (bUIHit)
                    //{
                    //    if (!dragging)
                    //        inputModule.PointerDown(hitTransform.gameObject,closestRay);
                    //    else
                    //        inputModule.PointerHold(hitTransform.gameObject, closestRay);
                    //    dragging = true;
                    //}
                        
                    
                }

                if (bHit && interactWithUI.GetStateUp(pose.inputSource))
                {
                    PointerEventArgs argsHold = new PointerEventArgs();
                    argsHold.fromInputSource = pose.inputSource;
                    argsHold.distance = hit.distance;
                    argsHold.flags = 0;
                    argsHold.target = hitTransform;
                    argsHold.hit = hit.point;
                    OnPointerClick(argsHold);
                    if(!bUIHit)
                        inputModule.Submit(hitTransform.gameObject);
                        
                }

                if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
                {
                    pointer.transform.localScale = new Vector3(thickness * 2f, dist / (transform.lossyScale.x * 2), thickness * 2f);
                }
                else
                {
                    pointer.transform.localScale = new Vector3(thickness, dist / (transform.lossyScale.x * 2), thickness);
                }
                pointer.GetComponent<MeshRenderer>().material.color = color;
                pointer.transform.localPosition = new Vector3(0f, 0f, dist / (transform.lossyScale.x * 2));
            }
            
        }
    }

    public struct PointerEventArgs
    {
        public SteamVR_Input_Sources fromInputSource;
        public uint flags;
        public float distance;
        public Transform target;
        public Vector3 hit;
    }

    public delegate void PointerEventHandler(object sender, PointerEventArgs e);
}