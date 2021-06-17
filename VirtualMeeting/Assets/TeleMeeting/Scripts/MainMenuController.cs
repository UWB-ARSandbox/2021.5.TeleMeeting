using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace TeleMeeting
{
    public class MainMenuController : MonoBehaviour
    {
        public Button resumeButton;
        public Button switchSceneButton;
        public Button leaveMeetingbutton;
        public Toggle micToggle;
        public Toggle verticalMovementToggle;
        private Vector3 menuMoveSpeed = new Vector3(0, 10, 0);
        private Vector3 menuDelta = Vector3.zero;
        private Vector3 menuOpenPosition;
        private Vector3 menuClosedPosition;
        private bool isOpen = false;
        private bool isVR = false;
        private bool menuActivelyLerping = false;

        private Coroutine menuLerping;
        
        public Action<int> OnSceneSwitch;
        public Action OnResumeClicked;
        public Action<bool> OnVerticalMovementToggled;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(resumeButton != null);
            Debug.Assert(switchSceneButton != null);
            Debug.Assert(leaveMeetingbutton != null);
            Debug.Assert(micToggle != null);
            Debug.Assert(verticalMovementToggle != null);

            resumeButton.onClick.AddListener(ToggleMenu);
            switchSceneButton.onClick.AddListener(SwitchSceneClicked);
            leaveMeetingbutton.onClick.AddListener(LeaveMeetingClicked);
            micToggle.onValueChanged.AddListener(MicMuted);
            verticalMovementToggle.onValueChanged.AddListener(VerticalMovementToggled);

            menuDelta = new Vector3(0, (Screen.height / 10) * 10, 0);

            isVR = MainController.getInstance().clientTypeVR;

            menuOpenPosition = transform.localPosition;
            menuClosedPosition = menuOpenPosition - menuDelta;
            if (!isVR)
                transform.localPosition = menuClosedPosition;
            //else
                //transform.localPosition = Vector3.zero;
        }

        private void OnGUI()
        {
            Event e = Event.current;
            if (!menuActivelyLerping)
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    //e.Use();
                    ToggleMenu();
                    
                }
                    
        }

        public void ToggleMenu()
        {
            if(!isVR)
            {
                if (menuLerping != null)
                    StopCoroutine(menuLerping);
                if (isOpen)
                {
                    menuActivelyLerping = true;
                    menuLerping = StartCoroutine(MenuLerp(menuClosedPosition));
                }

                else
                {
                    menuActivelyLerping = true;
                    menuLerping = StartCoroutine(MenuLerp(menuOpenPosition));
                    PC_CameraControl.instance.SetCursorLock(false);
                }
            }
            OnResumeClicked?.Invoke();
            
        }

        private void SwitchSceneClicked()
        {
            OnSceneSwitch?.Invoke(0);
            //FireBoxController.Instance.ChangeScene();
        }

        private void LeaveMeetingClicked()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private IEnumerator MenuLerp(Vector3 finalPosition)
        {
            bool moveFlag = true;
            while (moveFlag)
            {
                if(isOpen)
                {
                    if (transform.localPosition.y > finalPosition.y)
                    {
                        transform.localPosition -= menuMoveSpeed;
                    } else
                    {
                        moveFlag = false;
                        isOpen = false;
                    }
                }
                else
                {
                    if (transform.localPosition.y < finalPosition.y)
                    {
                        transform.localPosition += menuMoveSpeed;
                    }
                    else
                    {
                        moveFlag = false;
                        isOpen = true;
                    }
                }
                yield return null;
            }
            menuActivelyLerping = false;
        }

        private void MicMuted(bool state)
        {
            //Do stuff
            if (state)
            {
                RTCConference.Instance.Mute();
            }
            else
            {
                RTCConference.Instance.UnMute();
            }
            //Debug.Log("Mic Muted!");
        }

        private void VerticalMovementToggled(bool state)
        {
            OnVerticalMovementToggled?.Invoke(state);
        }

    }
}

