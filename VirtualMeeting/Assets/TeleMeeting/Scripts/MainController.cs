using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
using UnityEngine.XR;
using System;
using UnityEngine.SceneManagement;

namespace TeleMeeting
{
    public class MainController : MonoBehaviour
    {
        public static GameObject Player;
        public static int peer_id
        {
            get
            {
                return GameLiftManager.GetInstance().m_PeerId;
            }
        }
        private static MainController Instance = null;
        private static bool vrinitialized = false;
        public bool clientTypeVR = false;
        public Vector3 InitialPlayerPosition = new Vector3(0, 0, 0);

        private void Awake()
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach(string argument in args)
            {
                if (argument.CompareTo("LaunchVR") == 0)
                {
                    //Debug.LogError("Commandline argument is LaunchVR!");
                    clientTypeVR = true;
                }
                    
            }
            /*if (!startFireBox)
            {
                if(GetComponent<FireBoxController>())
                    GetComponent<FireBoxController>().enabled = false;
            }*/
            if (clientTypeVR == true)
            {
                //Debug.LogError(clientTypeVR);
                //Debug.LogError("ClientType is VR!");
                StartXR();
            }
                
                
            Instance = this;

        }
        private void Start()
        {
            if (clientTypeVR)
            {
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StartSubsystems();
                
            }
            else
            {
                InitialPlayerPosition += new Vector3(0, 2.5f, 0);
            }
            StartPlayer(clientTypeVR);
        }

        // Start is called before the first frame update
        void StartPlayer(bool clientType)
        {
            //Debug.Assert(player != null);
            //peer_id = GameLiftManager.GetInstance().m_PeerId;
            if (clientTypeVR)
            {

                if (XRSettings.loadedDeviceName.Equals("OpenVR Display"))
                {
                    clientTypeVR = true;
                    Player = Instantiate(Resources.Load("Prefabs/VRPlayer", typeof(GameObject))) as GameObject;
                    Player.transform.position = new Vector3(0, 1, -10);
                    Player.name = string.Format("VRPlayer{0}", GameLiftManager.GetInstance().m_PeerId);
                    Player.transform.position = InitialPlayerPosition;
                }

            }
            else
            {
                Player = Instantiate(Resources.Load("Prefabs/PCPlayer", typeof(GameObject))) as GameObject;
                Player.name = string.Format("PCPlayer{0}", GameLiftManager.GetInstance().m_PeerId);
                Camera.main.enabled = false;
                Player.GetComponent<Camera>().tag = "MainCamera";
                Player.transform.position = InitialPlayerPosition;
            }

            foreach (Camera obj in FindObjectsOfType<Camera>())
            {
                if (obj.gameObject.name.Equals("Main Camera"))
                {
                    obj.enabled = false;
                }
            }
        }

        public static MainController getInstance()
        {
            return Instance;
        }

        private bool VRPresent()
        {
            List<XRDisplaySubsystem> xrDisplaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
            foreach (XRDisplaySubsystem xrDisplay in xrDisplaySubsystems)
            {
                if (xrDisplay.running)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnApplicationQuit()
        {
            if(clientTypeVR)
                StopXR();
        }

        public void StartXR()
        {
            Debug.Log("Initializing XR...");

            if (!vrinitialized)
            {
                if (UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader == null)
                    UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader.Initialize();
                //Valve.VR.SteamVR.Initialize(true);
                vrinitialized = true;
            }
        }
        public IEnumerator StartXRSubSystems()
        {
            while(!UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                yield return false;
            }
        }
        public void StopXR()
        {
            Debug.Log("Stopping XR...");

            UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StopSubsystems();
            UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            Debug.Log("XR stopped completely.");
        }

    }
}

