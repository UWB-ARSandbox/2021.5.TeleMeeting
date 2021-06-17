using System.Collections;
using System.Collections.Generic;
using ASL;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.EventSystems;
namespace TeleMeeting
{
    public class PC_CameraControl : MonoBehaviour
    {
        public Camera mainCam;
        //marker size for the whiteboard
        public int markerSize = 2;
        //color of the pen for the whiteboard
        public Color drawingColor = Color.black;
        public ColorPickerController colorPicker;
        public ScreenSharePickerController screenPicker;
        public SceneSwitchingController sceneSwitcher;
        public MainMenuController mainMenu;

        private float drawTimer;
        public int draw_refresh_rate = 60;
        private float draw_refresh;
        public int draw_clear_cooldown = 1;
        private float draw_clear_last;
        private bool drawing;
        private int peer_id;

        public float rotationSpeed = 3f;
        public static readonly float MOVEMENT_SPEED = 10f;
        private bool cursorLock;
        private float camVertAngle = 0;
        private bool verticalMovement = false;

        public static PC_CameraControl instance;
        // Start is called before the first frame update
        void Start()
        {
            draw_refresh = 1 / draw_refresh_rate;
            draw_clear_last = Time.time;
            drawTimer = 0;
            drawing = false;
            peer_id = GameLiftManager.GetInstance().m_PeerId;
            cursorLock = false;

            colorPicker.OnColorChanged += ColorChanged;
            colorPicker.OnMarkerSizeChanged += MarkerSizeChanged;
            colorPicker.OnHiderClicked += ColorPickerHiderClicked;

            mainMenu.OnVerticalMovementToggled += VerticalMovementChanged;
            mainMenu.OnSceneSwitch += SceneSwitchClicked;

            if (instance == null)
                instance = this;
        }

        //public void createPlayer()
        //{
        //    ASLHelper.InstantiateASLObject(
        //        PrimitiveType.Cube,
        //        new Vector3(0, 0, 0),
        //        Quaternion.identity,
        //        null,
        //        null,
        //        OnPlayerCreated);
        //    StartCoroutine(DelayedInit());
        //    StartCoroutine(NetworkedUpdate());
        //}
        //private static void OnPlayerCreated(GameObject obj)
        //{
        //    playerSharedObject = obj;
        //    playerASLSharedObject = obj.GetComponent<ASLObject>();
        //}

        //IEnumerator DelayedInit()
        //{
        //    while (playerSharedObject == null)
        //    {
        //        yield return new WaitForSeconds(0.1f);
        //    }

        //    playerASLSharedObject.SendAndSetClaim(() =>
        //    {
        //        playerASLSharedObject.SendAndSetObjectColor(
        //            new Color(0.0f, 0.0f, 0.0f, 0.0f),
        //            new Color(0.2f, 0.4f, 0.2f));
        //    });

        //    playerSharedObject.SetActive(false);

        //}

        //IEnumerator NetworkedUpdate()
        //{
        //    while (true)
        //    {
        //        if (playerSharedObject == null)
        //            yield return new WaitForSeconds(0.1f);

        //        playerASLSharedObject.SendAndSetClaim(() =>
        //        {
        //            playerASLSharedObject.SendAndSetWorldPosition(mainCam.transform.position);
        //        });

        //        yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);
        //    }
        //}

        // Update is called once per frame
        void Update()
        {
            if(EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.transform.parent != null && EventSystem.current.currentSelectedGameObject.transform.GetComponentInParent<ChatController>() != null)
            {
                return;
            }
            PCInteractions();
            if (Input.GetKey(KeyCode.W))
            {
                if(verticalMovement)
                {
                    mainCam.transform.localPosition += (mainCam.transform.forward * MOVEMENT_SPEED * Time.deltaTime);
                } 
                else
                {
                    Vector3 flatForward = mainCam.transform.forward;
                    flatForward.y = 0;
                    flatForward = flatForward.normalized;
                    mainCam.transform.localPosition += (flatForward * MOVEMENT_SPEED * Time.deltaTime);
                }
                
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (verticalMovement)
                {
                    mainCam.transform.localPosition -= (mainCam.transform.forward * MOVEMENT_SPEED * Time.deltaTime);
                }
                else
                {
                    Vector3 flatForward = mainCam.transform.forward;
                    flatForward.y = 0;
                    flatForward = flatForward.normalized;
                    mainCam.transform.localPosition -= (flatForward * MOVEMENT_SPEED * Time.deltaTime);
                }
                //mainCam.transform.localPosition -= (mainCam.transform.forward * MOVEMENT_SPEED * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                if (verticalMovement)
                {
                    mainCam.transform.localPosition -= (mainCam.transform.right * MOVEMENT_SPEED * Time.deltaTime);
                }
                else
                {
                    Vector3 flatRight = mainCam.transform.right;
                    flatRight.y = 0;
                    flatRight = flatRight.normalized;
                    mainCam.transform.localPosition -= (flatRight * MOVEMENT_SPEED * Time.deltaTime);
                }
                //mainCam.transform.localPosition -= (mainCam.transform.right * MOVEMENT_SPEED * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                if (verticalMovement)
                {
                    mainCam.transform.localPosition += (mainCam.transform.right * MOVEMENT_SPEED * Time.deltaTime);
                }
                else
                {
                    Vector3 flatRight = mainCam.transform.right;
                    flatRight.y = 0;
                    flatRight = flatRight.normalized;
                    mainCam.transform.localPosition += (flatRight * MOVEMENT_SPEED * Time.deltaTime);
                }
                //mainCam.transform.localPosition += (mainCam.transform.right * MOVEMENT_SPEED * Time.deltaTime);
            }
            if (cursorLock)
            {
                float thetax = Input.GetAxis("Mouse X") * rotationSpeed;
                float thetay = Input.GetAxis("Mouse Y") * rotationSpeed;
                if (camVertAngle + thetay > 85 || camVertAngle + thetay < -85)
                {
                    thetay = 0;
                }
                camVertAngle += thetay;
                Quaternion rot =
                    Quaternion.AngleAxis(thetax, Vector3.up) *
                    Quaternion.AngleAxis(thetay, -mainCam.transform.right);
                mainCam.transform.localRotation = rot * mainCam.transform.localRotation;
            }
            //rotating camera with arrow keys
            if (Input.GetKey(KeyCode.RightArrow))
            {
                Quaternion rot = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);
                mainCam.transform.localRotation = rot * mainCam.transform.localRotation;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                Quaternion rot = Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, Vector3.up);
                mainCam.transform.localRotation = rot * mainCam.transform.localRotation;
            }

        }

        private void PCInteractions()
        {
            RaycastHit hit;
            if (Input.GetMouseButton(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        Vector3 pos = hit.point;

                        if (hit.transform.GetComponent<WhiteboardDrawing>() != null)
                        {
                            if (drawTimer > draw_refresh)
                            {
                                drawTimer = 0;
                                if (Input.GetKey(KeyCode.E))
                                {
                                    float[] values = new float[5];
                                    values[0] = pos.x;
                                    values[1] = pos.y;
                                    values[2] = pos.z;
                                    values[3] = drawing ? 1.0f : 0f;
                                    values[4] = markerSize;
                                    hit.transform.GetComponent<WhiteboardDrawing>()
                                        .EraseOnWhiteboard(values);
                                }
                                else
                                {
                                    float[] values = new float[8];
                                    values[0] = pos.x;
                                    values[1] = pos.y;
                                    values[2] = pos.z;
                                    values[3] = drawing ? 1.0f : 0f;
                                    values[4] = markerSize;
                                    values[5] = drawingColor.r;
                                    values[6] = drawingColor.g;
                                    values[7] = drawingColor.b;
                                    hit.transform.GetComponent<WhiteboardDrawing>()
                                        .DrawOnWhiteboard(values);
                                }
                                drawing = true;
                            }
                            else
                            {
                                drawTimer += Time.deltaTime;
                            }

                        }
                        //else if (hit.transform.GetComponent<uDesktopDuplication.Texture>() != null)
                        //{
                        //    Debug.LogWarning("Screen CLicked");
                        //    screenPicker.Activate(hit.transform.GetComponent<ProjectorController>());
                        //    //ProjectorController projector = hit.transform.parent.GetComponent<ProjectorController>();
                        //    //projector.ScreenActivated();
                        //}
                    }
                }

            }

            else
            {
                drawing = false;
                if (Input.GetMouseButtonDown(1))
                {
                    //cursorLock = !cursorLock;
                    SetCursorLock(!cursorLock);
                    
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    if ((Time.time - draw_clear_last) > draw_clear_cooldown)
                    {
                        draw_clear_last = Time.time;
                        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit))
                        {
                            if (hit.transform.GetComponent<WhiteboardDrawing>() != null)
                            {
                                hit.transform.GetComponent<WhiteboardDrawing>().ClearWhiteboard();
                            }
                        }
                    }


                }
                else if (Input.GetKeyDown(KeyCode.Tab))
                {

                    if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        if (hit.transform.GetComponent<uDesktopDuplication.Texture>() != null)
                        {
                            ProjectorController projector = hit.transform.parent.GetComponent<ProjectorController>();
                            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                            {
                                projector.ScreenChanged(false);
                            }
                            else
                            {
                                projector.ScreenChanged(true);
                            }
                        }
                    }
                }
            }
        }

        public void SetCursorLock(bool set)
        {
            cursorLock = set;
            if (cursorLock)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        public void ColorChanged(Color newColor)
        {
            drawingColor = newColor;
        }

        public void MarkerSizeChanged(int val)
        {
            markerSize = val;
        }

        public void ColorPickerHiderClicked()
        {

        }

        public void VerticalMovementChanged(bool state)
        {
            verticalMovement = state;
        }

        public void SceneSwitchClicked(int val)
        {
            mainMenu.ToggleMenu();
            //mainMenu.gameObject.SetActive(false);
            sceneSwitcher.GenerateSceneGrid(val);
            sceneSwitcher.gameObject.SetActive(true);
        }

    }
}

