using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

namespace TeleMeeting
{
    public class PlayerAvatar : MonoBehaviour
    {
        private bool clientTypeVR;
        internal static GameObject[] Avatar_objects;
        internal static ASLObject[] Avatar_ASLObjects;
        internal static Transform[] ParentObjects;
        private static readonly float UPDATES_PER_SECOND = 10f;
        private Quaternion hand_rot = Quaternion.AngleAxis(135, Vector3.right);
        
        // Start is called before the first frame update
        void Start()
        {
            clientTypeVR = MainController.getInstance().clientTypeVR;

            if(clientTypeVR)
            {
                Avatar_objects = new GameObject[3];
                Avatar_ASLObjects = new ASLObject[3];
                ParentObjects = new Transform[3];
                ParentObjects[0] = transform.GetChild(0).GetChild(3);
                ParentObjects[1] = transform.GetChild(0).GetChild(1);
                ParentObjects[2] = transform.GetChild(0).GetChild(2);
                createPlayerAvatarVR();
            } else
            {
                Avatar_objects = new GameObject[1];
                Avatar_ASLObjects = new ASLObject[1];
                ParentObjects = new Transform[1];
                ParentObjects[0] = transform;
                createPlayerAvatarPC();
                
            }

        }
        private void createPlayerAvatarVR()
        {
            ASLHelper.InstantiateASLObject(
                PrimitiveType.Cube,
                new Vector3(0, 0, 0),
                Quaternion.identity,
                null,
                "TeleMeeting.PlayerAvatarLocal",
                OnPlayerHeadCreated);
            ASLHelper.InstantiateASLObject(
                PrimitiveType.Capsule,
                new Vector3(0, 0, 0),
                Quaternion.identity,
                null,
                "TeleMeeting.PlayerAvatarLocal",
                OnPlayerLHandCreated);
            ASLHelper.InstantiateASLObject(
                PrimitiveType.Capsule,
                new Vector3(0, 0, 0),
                Quaternion.identity,
                null,
                "TeleMeeting.PlayerAvatarLocal",
                OnPlayerRHandCreated);
            StartCoroutine(DelayedInitVR());
            StartCoroutine(NetworkedUpdateVR());
        }
        private static void OnPlayerHeadCreated(GameObject obj)
        {
            Avatar_objects[0] = obj;
            Avatar_ASLObjects[0] = obj.GetComponent<ASLObject>();
            Avatar_ASLObjects[0].SendAndSetWorldScale(MainController.Player.transform.lossyScale / 6);
            Avatar_ASLObjects[0].GetComponent<Collider>().enabled = false;
        }

        private static void OnPlayerLHandCreated(GameObject obj)
        {
            Avatar_objects[1] = obj;
            Avatar_ASLObjects[1] = obj.GetComponent<ASLObject>();
            Avatar_ASLObjects[1].SendAndSetWorldScale(MainController.Player.transform.lossyScale / 6);
            Avatar_ASLObjects[1].GetComponent<Collider>().enabled = false;
        }

        private static void OnPlayerRHandCreated(GameObject obj)
        {
            Avatar_objects[2] = obj;
            Avatar_ASLObjects[2] = obj.GetComponent<ASLObject>();
            Avatar_ASLObjects[2].SendAndSetWorldScale(MainController.Player.transform.lossyScale / 6);
            Avatar_ASLObjects[2].GetComponent<Collider>().enabled = false;
        }

        IEnumerator DelayedInitVR()
        {
            while (Avatar_objects[0] == null || Avatar_objects[1] == null || Avatar_objects[2] == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Avatar_ASLObjects[0].SendAndSetClaim(() =>
            {
                Avatar_ASLObjects[0].SendAndSetObjectColor(
                    new Color(0.0f, 0.0f, 0.0f, 0.0f),
                    new Color(0.2f, 0.4f, 0.2f));
            });

            Avatar_ASLObjects[1].SendAndSetClaim(() =>
            {
                Avatar_ASLObjects[1].SendAndSetObjectColor(
                    new Color(0.0f, 0.0f, 0.0f, 0.0f),
                    new Color(0.2f, 0.4f, 0.2f));
            });

            Avatar_ASLObjects[2].SendAndSetClaim(() =>
            {
                Avatar_ASLObjects[2].SendAndSetObjectColor(
                    new Color(0.0f, 0.0f, 0.0f, 0.0f),
                    new Color(0.2f, 0.4f, 0.2f));
            });

            Avatar_objects[0].SetActive(false);
            Avatar_objects[1].SetActive(false);
            Avatar_objects[2].SetActive(false);
        }

        IEnumerator NetworkedUpdateVR()
        {
            while (true)
            {
                if (Avatar_objects[0] == null || Avatar_objects[1] == null || Avatar_objects[2] == null)
                    yield return new WaitForSeconds(0.1f);

                Avatar_ASLObjects[0].SendAndSetClaim(() =>
                {
                    Avatar_ASLObjects[0].SendAndSetWorldPosition(ParentObjects[0].position);
                    Avatar_ASLObjects[0].SendAndSetWorldRotation(ParentObjects[0].rotation);
                });

                Avatar_ASLObjects[1].SendAndSetClaim(() =>
                {
                    Avatar_ASLObjects[1].SendAndSetWorldPosition(ParentObjects[1].position);
                    Avatar_ASLObjects[1].SendAndSetWorldRotation(ParentObjects[1].rotation * hand_rot);
                });

                Avatar_ASLObjects[2].SendAndSetClaim(() =>
                {
                    Avatar_ASLObjects[2].SendAndSetWorldPosition(ParentObjects[2].position);
                    Avatar_ASLObjects[2].SendAndSetWorldRotation(ParentObjects[2].rotation * hand_rot);
                });

                yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);
            }
        }

        private void createPlayerAvatarPC()
        {
            ASLHelper.InstantiateASLObject(
                PrimitiveType.Cube,
                new Vector3(0, 0, 0),
                Quaternion.identity,
                null,
                "TeleMeeting.PlayerAvatarLocal",
                OnPlayerHeadCreated);
            StartCoroutine(DelayedInitPC());
            StartCoroutine(NetworkedUpdatePC());
        }

        IEnumerator DelayedInitPC()
        {
            while (Avatar_objects[0] == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Avatar_ASLObjects[0].SendAndSetClaim(() =>
            {
                Avatar_ASLObjects[0].SendAndSetObjectColor(
                    new Color(0.0f, 0.0f, 0.0f, 0.0f),
                    new Color(0.2f, 0.4f, 0.2f));
            });

            Avatar_objects[0].SetActive(false);
        }

        IEnumerator NetworkedUpdatePC()
        {
            while (true)
            {
                if (Avatar_objects[0] == null)
                    yield return new WaitForSeconds(0.1f);

                Avatar_ASLObjects[0].SendAndSetClaim(() =>
                {
                    Avatar_ASLObjects[0].SendAndSetWorldPosition(transform.position);
                    Avatar_ASLObjects[0].SendAndSetWorldRotation(transform.rotation);
                });

                yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);
            }
        }
    }
    public class PlayerAvatarLocal : MonoBehaviour
    {
        private PrimitiveType type;
        private GameObject parentAvatarPart;
        private GameObject localAvatarPart;

        private static readonly float INTERPOLATION_PER_FRAME = 0.05f;

        void Start()
        {
            Debug.LogWarning("Local Start was called!");
            parentAvatarPart = gameObject;
            if (parentAvatarPart.transform.parent == null)
            {
                parentAvatarPart.transform.SetParent(GameObject.Find("PlayerAvatars").transform);
            }
            
            Debug.LogWarning(gameObject.GetComponent<MeshRenderer>().name);
            foreach (GameObject obj in PlayerAvatar.Avatar_objects)
            {
                if (obj == parentAvatarPart)
                {
                    Destroy(this);
                    return;
                }
            }
            switch(parentAvatarPart.gameObject.GetComponent<MeshRenderer>().name)
            {
                case "Cube":
                    type = PrimitiveType.Cube;
                    break;
                case "Capsule":
                    type = PrimitiveType.Capsule;
                    break;
                default:
                    Debug.LogError("Avatar of Unrecognized Type!");
                    break;
            }
            
            localAvatarPart = GameObject.CreatePrimitive(type);
            localAvatarPart.transform.SetParent(transform.parent);
            localAvatarPart.transform.localScale = parentAvatarPart.transform.localScale;
            localAvatarPart.GetComponent<Renderer>().material = parentAvatarPart.GetComponent<Renderer>().material;
            localAvatarPart.GetComponent<Collider>().enabled = false;
            parentAvatarPart.GetComponent<Renderer>().forceRenderingOff = true;
            parentAvatarPart.GetComponent<Collider>().enabled = false;
        }

        private void Update()
        {
            localAvatarPart.transform.position = Vector3.Lerp(localAvatarPart.transform.position, parentAvatarPart.transform.position, INTERPOLATION_PER_FRAME);
            localAvatarPart.transform.rotation = Quaternion.Lerp(localAvatarPart.transform.rotation, parentAvatarPart.transform.rotation, INTERPOLATION_PER_FRAME);
            localAvatarPart.transform.localScale = parentAvatarPart.transform.localScale;
        }
    }
}

