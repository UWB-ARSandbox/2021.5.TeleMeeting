using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

namespace TeleMeeting
{
    public class HandController : MonoBehaviour
    {

        public enum MenuOptions
        {
            None = -1,
            Main,
            ColorPicker
            //LaserPointer
        }

        private SteamVR_LaserPointer laserPointer;
        private Hand hand;
        //private SteamVR_Input_Sources inputSources;
        public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
        public SteamVR_Action_Boolean grabGripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
        public SteamVR_Action_Boolean uiInteractAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
        public SteamVR_Action_Boolean MenuPressAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("MenuPress");
        public SteamVR_Action_Vector2 StickAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("Stick");
        public HandController otherhandcontroller;

        public Canvas UICanvas;
        public ColorPickerController ColorPickerMenu;
        //public GameObject ScreenSharePickerMenu;
        public LaserPointerUIController LaserPointerMenu;
        public MainMenuController MainMenu;
        public SceneSwitchingController SceneSwitch;
        public Color HoverAlphaShift = new Color(0, 0, 0, 0.4f);
        private bool menuState = false;
        private bool pointerHoverFlag = false;
        private Transform pointedObject = null;
        private bool shouldLaserPointForObject = false;
        private bool shouldLaserPointForUI = false;
        public VRInputScript inputModule;

        public Transform lastActiveObject = null;

        private MenuOptions lastOpenMenu = MenuOptions.None;

        private Interactable handHoveringObject
        {
            get
            {
                return hand.hoveringInteractable;
            }
        }

        [SerializeField] private Camera handCamera;
        
        // Start is called before the first frame update
        void Start()
        {
            //hand.
            //Debug.Assert(UICanvas != null);
            laserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
            laserPointer.PointerIn += PointerInside;
            laserPointer.PointerOut += PointerOutside;
            laserPointer.PointerClick += PointerClick;
            laserPointer.PointerHold += PointerHold;
            laserPointer.Pointer += Pointer;

            laserPointer.setActive(menuState);

            hand = gameObject.GetComponent<Hand>();
            //inputSources = hand.gameObject.name == "LeftHand" ? inputSources = SteamVR_Input_Sources.LeftHand : inputSources = SteamVR_Input_Sources.RightHand;

            uiInteractAction.AddOnChangeListener(InteractedUI, hand.handType);
            grabGripAction.AddOnChangeListener(GripGrabbed, hand.handType);
            grabPinchAction.AddOnChangeListener(GripGrabbed, hand.handType);
            MenuPressAction.AddOnStateUpListener(MenuTriggered, hand.handType);
            //MenuPressAction.AddOnStateUpListener(OtherMenuTriggered, hand.otherHand.handType);
            if (hand.handType == SteamVR_Input_Sources.LeftHand)
                StickAction.AddOnAxisListener(JoyStickMoved, hand.handType);
            MainMenu.OnResumeClicked += MenuClosed;
            MainMenu.OnSceneSwitch += GenerateScenes;

            SceneSwitch.onMenuClose += MenuClosed;
            SceneSwitch.switchHandler.AddSceneSwitchCallback(SceneLoaded);

            //MainMenu.transform.localPosition = Vector3.zero;
            //ColorPickerMenu.transform.localPosition = Vector3.zero;

        }
        public struct PointerClickValues
        {
            public object sender;
            public PointerEventArgs pointerEvent;
            public Hand pointerHand;
            public SteamVR_LaserPointer laserPointer;
        }
        public void PointerClick(object sender, PointerEventArgs e)
        {
            PointerClickValues values = new PointerClickValues();
            values.pointerEvent = e;
            values.sender = sender;
            values.pointerHand = hand;
            values.laserPointer = laserPointer;
            e.target.SendMessage("PointerClicked", values, SendMessageOptions.DontRequireReceiver);
            lastActiveObject = e.target;

        }

        public void PointerHold(object sender, PointerEventArgs e)
        {
            PointerClickValues values = new PointerClickValues();
            values.pointerEvent = e;
            values.sender = sender;
            values.pointerHand = hand;
            values.laserPointer = laserPointer;
            e.target.SendMessage("PointerHeld", values, SendMessageOptions.DontRequireReceiver);
            lastActiveObject = e.target;
        }

        public void Pointer(object sender, PointerEventArgs e)
        {
            PointerClickValues values = new PointerClickValues();
            values.pointerEvent = e;
            values.sender = sender;
            values.pointerHand = hand;
            values.laserPointer = laserPointer;
            e.target.SendMessage("PointerHover", values, SendMessageOptions.DontRequireReceiver);
        }

        public void PointerInside(object sender, PointerEventArgs e)
        {
            if (!pointerHoverFlag)
                laserPointer.color = laserPointer.color += HoverAlphaShift;
            pointerHoverFlag = true;
            pointedObject = e.target;
        }

        public void PointerOutside(object sender, PointerEventArgs e)
        {
            PointerClickValues values = new PointerClickValues();
            values.pointerEvent = e;
            values.sender = sender;
            values.pointerHand = hand;
            values.laserPointer = laserPointer;
            laserPointer.color = laserPointer.color -= HoverAlphaShift;
            pointerHoverFlag = false;
            pointedObject = null;
            e.target.SendMessage("PointerOut", values, SendMessageOptions.DontRequireReceiver);


        }
        //For our current interaction set, this function call is actually useless. We should just use PointerClick.
        public void InteractedUI(SteamVR_Action_Boolean action, SteamVR_Input_Sources source, bool value)
        {

        }

        public void GenerateScenes(int index)
        {
            MainMenu.gameObject.SetActive(false);
            SceneSwitch.GenerateSceneGrid(index);
            SceneSwitch.gameObject.SetActive(true);
        }

        public void GripGrabbed(SteamVR_Action_Boolean action, SteamVR_Input_Sources source, bool value)
        {
            if(value)
            {
                if (handHoveringObject != null)
                {
                    if (handHoveringObject.gameObject == otherhandcontroller.hand.currentAttachedObject)
                    {
                        otherhandcontroller.shouldLaserPointForObject = false;
                        otherhandcontroller.handCamera.transform.parent = otherhandcontroller.transform;
                        otherhandcontroller.handCamera.transform.localPosition = Vector3.zero;
                        otherhandcontroller.handCamera.transform.localRotation = Quaternion.identity;
                        otherhandcontroller.TryShouldLaserPoint();
                    }
                    if(action == grabGripAction)
                        hand.AttachObject(hand.hoveringInteractable.gameObject, GrabTypes.Grip);
                    else
                        hand.AttachObject(hand.hoveringInteractable.gameObject, GrabTypes.Pinch);
                    handCamera.transform.parent = hand.currentAttachedObject.transform;
                    handCamera.transform.localPosition = Vector3.zero;
                    handCamera.transform.localRotation = Quaternion.identity;
                    if (hand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardPen)
                    {
                        shouldLaserPointForObject = true;
                        laserPointer.color = Color.red;
                    }
                    else if (hand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardEraser)
                    {
                        shouldLaserPointForObject = true;
                        laserPointer.color = Color.red;
                    }
                    else if (hand.currentAttachedObject == IndexPlayerIntereractions.instance.laserPointer)
                    {
                        shouldLaserPointForObject = true;
                        laserPointer.color = Color.black;
                    }
                }
            } else
            {
                if (hand.currentAttachedObject != null)
                {
                    GameObject handObject = hand.currentAttachedObject;
                    hand.DetachObject(hand.currentAttachedObject, true);
                    handCamera.transform.parent = transform;
                    handCamera.transform.localPosition = Vector3.zero;
                    handCamera.transform.localRotation = Quaternion.identity;
                    laserPointer.color = Color.black;
                    shouldLaserPointForObject = false;
                    if(handObject == IndexPlayerIntereractions.instance.whiteboardPen)
                    {
                        handObject.transform.parent = IndexPlayerIntereractions.instance.leftHandController.transform;
                        handObject.transform.localPosition = IndexPlayerIntereractions.instance.PenOriginalPosition;
                        handObject.transform.localRotation = IndexPlayerIntereractions.instance.PenOriginalRotation;
                    } else if (handObject == IndexPlayerIntereractions.instance.whiteboardEraser)
                    {
                        handObject.transform.parent = IndexPlayerIntereractions.instance.leftHandController.transform;
                        handObject.transform.localPosition = IndexPlayerIntereractions.instance.EraserOriginalPosition;
                        handObject.transform.localRotation = IndexPlayerIntereractions.instance.EraserOriginalRotation;
                    } else if (handObject == IndexPlayerIntereractions.instance.laserPointer)
                    {
                        handObject.transform.parent = IndexPlayerIntereractions.instance.rightHandController.transform;
                        handObject.transform.localPosition = IndexPlayerIntereractions.instance.PointerOriginalPosition;
                        handObject.transform.localRotation = IndexPlayerIntereractions.instance.PointerOriginalRotation;
                    }
                    if(lastActiveObject != null)
                        lastActiveObject.SendMessage("GripReleased", SendMessageOptions.DontRequireReceiver);
                }
            }
            TryShouldLaserPoint();
            
        }

        //public void MenuTriggered(SteamVR_Action_Boolean action, SteamVR_Input_Sources source, bool value)
        //{
        //    //New Code for Multi-Menu Support
        //    //Leave unreachable until all menu elements are implemented
        //    menuState = !menuState;
        //    MenuOptions openMenu;
        //    //Check if an object is being held in the other hand
        //    if (hand.otherHand.currentAttachedObject == IndexPlayerIntereractions.whiteboardPen ||
        //        hand.otherHand.currentAttachedObject == IndexPlayerIntereractions.whiteboardEraser)
        //        openMenu = MenuOptions.ColorPicker;
        //    //else if (hand.otherHand.currentAttachedObject == IndexPlayerIntereractions.laserPointer)
        //    //    openMenu = MenuOptions.LaserPointer;
        //    else
        //        openMenu = MenuOptions.Main;
        //    //Turn off last menu if opening menu
        //    if(menuState)
        //    {
        //        //Turn off last menu
        //        if (lastOpenMenu != MenuOptions.None)
        //        {
        //            switch (lastOpenMenu)
        //            {
        //                case MenuOptions.Main:
        //                    MainMenu.gameObject.SetActive(false);
        //                    break;
        //                case MenuOptions.ColorPicker:
        //                    ColorPickerMenu.gameObject.SetActive(false);
        //                    break;
        //            }
        //        }
        //        //Turn on Canvas
        //        UICanvas.gameObject.SetActive(true);

        //        //Turn on appropriate Menu
        //        //UICanvas.transform.GetChild((int)openMenu).gameObject.SetActive(true);
        //        switch(openMenu)
        //        {
        //            case MenuOptions.Main:
        //                MainMenu.gameObject.SetActive(true);
        //                break;
        //            case MenuOptions.ColorPicker:
        //                ColorPickerMenu.gameObject.SetActive(true);
        //                break;
        //        }
        //        lastOpenMenu = openMenu;
        //        //Close other hand's menu if it's open
        //        if (otherhandcontroller.menuState)
        //            MenuTriggered(MenuPressAction, hand.handType, false);
        //        TryToggleLaserPointer(false);
        //        otherhandcontroller.TryToggleLaserPointer(true);
        //    } else
        //    {
        //        UICanvas.gameObject.SetActive(false);

        //    }

        //    //Turn off this hand's Laser Pointer
        //    TryToggleLaserPointer(!menuState);

        //    //Turn on otherHand's Laser Pointer
        //    otherhandcontroller.TryToggleLaserPointer(menuState);
        //}

        private void MenuClosed()
        {
            MenuTriggered(uiInteractAction, hand.handType);
        }

        public void MenuTriggered(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
        {
            Debug.Log("Menu Triggered!");
            Debug.Log(source.ToString());
            //New Code for Multi-Menu Support
            //Leave unreachable until all menu elements are implemented
            menuState = !menuState;
            Debug.Log(menuState);
            MenuOptions openMenu;
            //Check if an object is being held in the other hand
            if (hand.otherHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardPen ||
                hand.otherHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardEraser)
                openMenu = MenuOptions.ColorPicker;
            //else if (hand.otherHand.currentAttachedObject == IndexPlayerIntereractions.laserPointer)
            //    openMenu = MenuOptions.LaserPointer;
            else
                openMenu = MenuOptions.Main;

            UICanvas.gameObject.SetActive(menuState);

            //Do stuff if the menu is open
            if (menuState)
            {
                //Turn off last menu
                if (lastOpenMenu != MenuOptions.None)
                {
                    switch (lastOpenMenu)
                    {
                        case MenuOptions.Main:
                            MainMenu.gameObject.SetActive(false);
                            break;
                        case MenuOptions.ColorPicker:
                            ColorPickerMenu.gameObject.SetActive(false);
                            break;
                    }
                }

                //Turn on appropriate Menu
                switch (openMenu)
                {
                    case MenuOptions.Main:
                        MainMenu.gameObject.SetActive(true);
                        break;
                    case MenuOptions.ColorPicker:
                        ColorPickerMenu.gameObject.SetActive(true);
                        break;
                }
                lastOpenMenu = openMenu;
                shouldLaserPointForUI = false;
                TryShouldLaserPoint();

            }
            //Tell the other hand my menu is open or closed
            otherhandcontroller.OtherMenuTriggered();
        }

        public void OtherMenuTriggered()
        {
            if(otherhandcontroller.menuState)
            {
                menuState = false;
                UICanvas.gameObject.SetActive(false);
                shouldLaserPointForUI = true;
                otherhandcontroller.shouldLaserPointForUI = false;
            }
            else
            {
                shouldLaserPointForUI = false;
            }
            TryShouldLaserPoint();
        }

        public void TryShouldLaserPoint()
        {
            if (shouldLaserPointForObject || shouldLaserPointForUI)
                laserPointer.setActive(true);
            else
                laserPointer.setActive(false);
            
        }

        public void JoyStickMoved(SteamVR_Action_Vector2 action, SteamVR_Input_Sources source, Vector2 axis, Vector2 delta)
        {
            Vector2 movement = axis;
            Vector3 forward_noz_normal = hand.transform.forward;
            if(!IndexPlayerIntereractions.instance.verticalMovement)
            {
                forward_noz_normal.y = 0;
                forward_noz_normal = forward_noz_normal.normalized;
            }
            

            Vector3 right_noz_normal = hand.transform.right;
            if(!IndexPlayerIntereractions.instance.verticalMovement)
            {
                right_noz_normal.y = 0;
                right_noz_normal = right_noz_normal.normalized;
            }
            
            MainController.Player.transform.position += forward_noz_normal * movement.y * Time.deltaTime * IndexPlayerIntereractions.MOVEMENT_SPEED;
            MainController.Player.transform.position += right_noz_normal * movement.x * Time.deltaTime * IndexPlayerIntereractions.MOVEMENT_SPEED;
        }

        public void SceneLoaded(string name)
        {
            Debug.Log("Scene Loaded Called!");
            MenuClosed();
        }
    }

}
