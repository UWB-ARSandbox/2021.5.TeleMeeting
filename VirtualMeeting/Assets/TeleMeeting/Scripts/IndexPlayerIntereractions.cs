using System;
using System.Collections;
using System.Collections.Generic;
using ASL;
using uDesktopDuplication;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
namespace TeleMeeting
{
    public class IndexPlayerIntereractions : MonoBehaviour
    {
        //marker size for the whiteboard
        public static int markerSize;
        //color of the pen for the whiteboard
        public static Color drawingColor;
        public static VRInputScript inputModule;
        //public Canvas pauseMenuCanvas;
        //public ColorPickerController colorPicker;

        public HandController leftHandController;
        public HandController rightHandController;

        public static readonly float DRAWING_DISTANCE = 2f;

        private SteamVR_Input_Sources leftHand = SteamVR_Input_Sources.LeftHand;
        private SteamVR_Input_Sources rightHand = SteamVR_Input_Sources.RightHand;
        GameObject lHand;
        GameObject rHand;

        //Private objects for Player
        public GameObject whiteboardPen;
        public GameObject whiteboardEraser;
        public GameObject laserPointer;

        private GameObject activeProjector;
        private bool drawing;
        private bool screenSharingActive;

        private static GameObject playerHeadSharedObject;
        private static ASL.ASLObject playerHeadASLSharedObject;

        private static GameObject playerLHandSharedObject;
        private static ASL.ASLObject playerLHandASLSharedObject;

        private static GameObject playerRHandSharedObject;
        private static ASL.ASLObject playerRHandASLSharedObject;

        private static readonly float UPDATES_PER_SECOND = 20f;
        public static float MOVEMENT_SPEED = 10f;
        private static readonly float pointerLengthReset = 100f;
        private float pointerLength = 100f;
        public Player player;

        private Vector3 beltPos;
        private Vector3 beltForward;
        private Vector3 beltRight;
        private static IndexPlayerIntereractions _instance;
        public static IndexPlayerIntereractions instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<IndexPlayerIntereractions>();
                }
                return _instance;
            }
        }

        public Vector3 PenOriginalPosition;
        public Quaternion PenOriginalRotation;
        public Vector3 EraserOriginalPosition;
        public Quaternion EraserOriginalRotation;
        public Vector3 PointerOriginalPosition;
        public Quaternion PointerOriginalRotation;

        public bool verticalMovement
            {get; private set;} = false;

        void Start()
        {
            //RTCCall.GetEngine().MuteLocalAudio(true);
            //RTCCall.GetEngine().MuteLocalVideo(true);
            screenSharingActive = false;
            //GameObject ColorImage = pauseMenuCanvas.transform.GetChild(12).gameObject;
            //ColorImage.GetComponent<Image>().color = Color.black;
            markerSize = 2;
            drawingColor = Color.black;
            drawing = false;
            lHand = transform.GetChild(0).GetChild(1).gameObject;
            rHand = transform.GetChild(0).GetChild(2).gameObject;
            player = GetComponent<Player>();
            //colorPicker.OnColorChanged += ColorChanged;
            //colorPicker.OnMarkerSizeChanged += MarkerChange;

            leftHandController.ColorPickerMenu.OnColorChanged += ColorChanged;
            leftHandController.ColorPickerMenu.OnColorChanged += rightHandController.ColorPickerMenu.SetColor;

            rightHandController.ColorPickerMenu.OnColorChanged += ColorChanged;
            rightHandController.ColorPickerMenu.OnColorChanged += leftHandController.ColorPickerMenu.SetColor;

            leftHandController.ColorPickerMenu.OnMarkerSizeChanged += MarkerChange;
            leftHandController.ColorPickerMenu.OnMarkerSizeChanged += rightHandController.ColorPickerMenu.setMarkerSize;

            rightHandController.ColorPickerMenu.OnMarkerSizeChanged += MarkerChange;
            rightHandController.ColorPickerMenu.OnMarkerSizeChanged += leftHandController.ColorPickerMenu.setMarkerSize;

            leftHandController.MainMenu.OnVerticalMovementToggled += VerticalMovementChanged;
            rightHandController.MainMenu.OnVerticalMovementToggled += VerticalMovementChanged;

            PenOriginalPosition = whiteboardPen.transform.localPosition;
            PenOriginalRotation = whiteboardPen.transform.localRotation;
            EraserOriginalPosition = whiteboardEraser.transform.localPosition;
            EraserOriginalRotation = whiteboardEraser.transform.localRotation;
            PointerOriginalPosition = laserPointer.transform.localPosition;
            PointerOriginalRotation = laserPointer.transform.localRotation;
    }

        // Update is called once per frame
        void Update()
        {
            //Vector3 feetoffset = (player.hmdTransform.position - player.feetPositionGuess);
            //beltPos = player.hmdTransform.position - (feetoffset * 0.3f);
            //beltForward = player.bodyDirectionGuess.normalized;
            //beltRight = Vector3.Cross(beltForward, Vector3.up).normalized;
            //if (!whiteboardPen.GetComponent<Interactable>().attachedToHand)
            //{
            //    //whiteboardPen.transform.position = beltPos + (beltRight * 0.2f * transform.localScale.z) + (beltForward * 0.025f * transform.localScale.x);
            //    //whiteboardPen.transform.rotation = Quaternion.FromToRotation(Vector3.forward, beltForward);

            //}
            //if (!whiteboardEraser.GetComponent<Interactable>().attachedToHand)
            //{
            //    //whiteboardEraser.transform.position = beltPos + (beltRight * 0.1f * transform.localScale.z) + (beltForward * 0.025f * transform.localScale.x);
            //    //whiteboardEraser.transform.rotation = Quaternion.FromToRotation(Vector3.forward, beltForward);
            //}
            //if (!laserPointer.GetComponent<Interactable>().attachedToHand)
            //{
            //    //laserPointer.transform.position = beltPos - (beltRight * 0.1f * transform.localScale.z) + (beltForward * 0.025f * transform.localScale.x);
            //    //laserPointer.transform.rotation = Quaternion.FromToRotation(Vector3.forward, beltForward);
            //}

            //if (!pauseMenuCanvas.enabled)
            //{
            //    Interactable hoveringObject = rHand.GetComponent<Hand>().hoveringInteractable;
            //    if (SteamVR_Input.GetStateDown("GrabGrip", rightHand))
            //    {
            //        if (hoveringObject)
            //        {
            //            rHand.GetComponent<Hand>().AttachObject(hoveringObject.gameObject, GrabTypes.Grip);
            //        }
            //    }
            //    else if (SteamVR_Input.GetStateUp("GrabGrip", rightHand))
            //    {
            //        GameObject attachedObject = rHand.GetComponent<Hand>().currentAttachedObject;
            //        if (attachedObject)
            //        {
            //            if (attachedObject.tag.Equals("ScreenSharingItem"))
            //            {
            //                ClearScreenSharing();
            //                screenSharingActive = false;
            //            }

            //            rHand.GetComponent<Hand>().DetachObject(attachedObject);

            //        }

            //        lineRend.positionCount = 0;
            //    }
            //}

            //Vector2 movement = SteamVR_Input.GetVector2("Stick", leftHand);
            //if (movement.magnitude != 0)
            //{
            //    Vector3 forward_noz_normal = lHand.transform.forward;
            //    forward_noz_normal.y = 0;
            //    forward_noz_normal = forward_noz_normal.normalized;

            //    Vector3 right_noz_normal = lHand.transform.right;
            //    right_noz_normal.y = 0;
            //    right_noz_normal = right_noz_normal.normalized;

            //    transform.position += forward_noz_normal * movement.y * Time.deltaTime * MOVEMENT_SPEED;
            //    transform.position += right_noz_normal * movement.x * Time.deltaTime * MOVEMENT_SPEED;

            //}

            //if (SteamVR_Input.GetStateDown("MenuPress", leftHand))
            //{
            //    pauseMenuCanvas.enabled = !pauseMenuCanvas.enabled;
            //}

            //if (pauseMenuCanvas.enabled)
            //{
            //    MenuInteraction();
            //}
            //else
            //{
            //    pointerLength = pointerLengthReset;
            //    lineRend.positionCount = 0;
            //}



            //GameObject holdingObject = rHand.GetComponent<Hand>().currentAttachedObject;
            //if (holdingObject && (holdingObject.tag.Equals("Pen") || holdingObject.tag.Equals("Eraser")))
            //{
            //    DrawingOnWhiteboard(holdingObject);
            //}
            //if (holdingObject && holdingObject.tag.Equals("ScreenSharingItem"))
            //{
            //    ScreenSharingInteraction();
            //}

        }

        //private void DrawingOnWhiteboard(GameObject WhiteboardUtil)
        //{
        //    Vector3 start = rHand.transform.position;
        //    Vector3[] vals = new Vector3[3];
        //    vals[0] = -rHand.transform.up;
        //    vals[1] = -rHand.transform.right;
        //    vals[2] = rHand.transform.forward;
        //    Vector3 end = vals[0] + vals[1] + vals[2];
        //    end = start + end.normalized * pointerLength;


        //    RaycastHit hit;
        //    if (Physics.Linecast(start, end, out hit))
        //    {
        //        lineRend.positionCount = 2;
        //        lineRend.SetPosition(0, start);
        //        lineRend.SetPosition(1, hit.point);
        //        float dis = Mathf.Abs(Vector3.Distance(start, hit.point));
        //        if (dis > 0.75f)
        //        {
        //            drawing = false;
        //            lineRend.startColor = Color.red;
        //            lineRend.endColor = Color.red;
        //            return;
        //        }
        //        lineRend.startColor = Color.green;
        //        lineRend.endColor = Color.green;
        //        if (hit.transform.GetComponent<WhiteboardDrawing>() != null)
        //        {
        //            if (WhiteboardUtil.tag.Equals("Pen"))
        //            {
        //                if (SteamVR_Input.GetState("InteractUI", rightHand))
        //                {
        //                    float[] values = new float[8];
        //                    values[0] = hit.point.x;
        //                    values[1] = hit.point.y;
        //                    values[2] = hit.point.z;
        //                    values[3] = drawing ? 1.0f : 0;
        //                    values[4] = markerSize;
        //                    values[5] = drawingColor.r;
        //                    values[6] = drawingColor.g;
        //                    values[7] = drawingColor.b;
        //                    hit.transform.GetComponent<WhiteboardDrawing>()
        //                        .DrawOnWhiteboard(values, GameLiftManager.GetInstance().m_PeerId);
        //                    drawing = true;
        //                }
        //                else
        //                {
        //                    drawing = false;
        //                }
        //            }
        //            else if (WhiteboardUtil.tag.Equals("Eraser"))
        //            {
        //                if (SteamVR_Input.GetState("InteractUI", rightHand))
        //                {
        //                    float[] values = new float[5];
        //                    values[0] = hit.point.x;
        //                    values[1] = hit.point.y;
        //                    values[2] = hit.point.z;
        //                    values[3] = drawing ? 1.0f : 0;
        //                    values[4] = markerSize;
        //                    hit.transform.GetComponent<WhiteboardDrawing>()
        //                        .EraseOnWhiteboard(values, GameLiftManager.GetInstance().m_PeerId);
        //                    drawing = true;
        //                }
        //                else
        //                {
        //                    drawing = false;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        lineRend.positionCount = 2;
        //        lineRend.SetPosition(0, start);
        //        lineRend.SetPosition(1, end);
        //        lineRend.startColor = Color.red;
        //        lineRend.endColor = Color.red;
        //    }
        //}

        private void ScreenSharingInteraction()
        {
            Vector3 start = rHand.transform.position;
            Vector3 end = start + rHand.transform.forward * pointerLength;

            RaycastHit hit;
            if (Physics.Linecast(start, end, out hit))
            {
                if (hit.transform.GetComponent<uDesktopDuplication.Texture>() != null)
                {
                    // put this code in when actually assigning the project to a screen
                    // ProjectorController projector = hit.transform.parent.GetComponent<ProjectorController>();
                    // put this code in afterwards to activate the projector
                    //projector.ScreenActivated();
                    if (SteamVR_Input.GetStateDown("InteractUI", rightHand))
                    {
                        

                    }
                }
            }
        }

        //private void MenuInteraction()
        //{
        //    Vector3 start = rHand.transform.position;

        //    PointerEventData data = inputModule.GetData();
        //    pointerLength = data.pointerCurrentRaycast.distance == 0 ? pointerLengthReset : data.pointerCurrentRaycast.distance;
        //    Vector3 end = start + rHand.transform.forward * pointerLength;
        //    lineRend.positionCount = 2;
        //    lineRend.SetPosition(0, start);
        //    lineRend.SetPosition(1, end);
        //    lineRend.startColor = Color.black;
        //    lineRend.endColor = Color.black;

        //}

        private void ColorChanged(Color newColor)
        {
            drawingColor = newColor;
        }

        private void MarkerChange(int newMarkerSize)
        {
            markerSize = newMarkerSize;
        }

        


        private void ClearScreenSharing()
        {
            if (!screenSharingActive)
                return;
            for (int i = uDesktopDuplication.Manager.monitorCount - 1; i >= 0; i--)
            {
                Destroy(player.transform.GetChild(player.transform.childCount - 1 - i).gameObject);
            }

        }

        public void VerticalMovementChanged(bool state)
        {
            verticalMovement = state;
        }
    }
}

