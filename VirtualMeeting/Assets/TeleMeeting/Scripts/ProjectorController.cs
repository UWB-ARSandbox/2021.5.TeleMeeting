using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Unity.Jobs;

namespace TeleMeeting
{
    public class ProjectorController : MonoBehaviour
    {
        public uDesktopDuplication.Texture texScript = null;
        public ASL.ASLObject screen = null;
        public bool active
        { get; private set; }
        private float timer = 0;
        float refreshrate = 30f;
        private float refreshgap;

        private List<GameObject> monitorGeneratedBoards;

        private float[] msgArr = { -1 };
        private Material screen_mat;
        public Material receive_mat;
        public int resolutiondownscale_ = 2;

        public Texture2D tex_;

        private NativeArray<byte> gpu_request_array;
        private NativeArray<byte> tex_array;
        private NativeArray<byte> jpg_array;

        private Byte[] received_img_array = null;

        public Texture2D ReceiveTex;
        private Action<AsyncGPUReadbackRequest> onAsyncGPURead;
        private Hash128 prevTexHash;

        CalculateFrameJob calculateFrameJobData;
        JobHandle calculateFrameHandle;


        private static readonly int BURST_MESSAGE_MAX = 4;
        private static readonly int MESSAGE_HEADER_FLOAT_SIZE = 3;
        private static readonly int MESSAGE_HEADER_BYTE_SIZE = MESSAGE_HEADER_FLOAT_SIZE * sizeof(float);

        private static readonly int MESSAGE_MAX_FLOAT_SIZE = 1000 - MESSAGE_HEADER_FLOAT_SIZE;
        private static readonly int MESSAGE_MAX_BYTE_SIZE = MESSAGE_MAX_FLOAT_SIZE * sizeof(float);


        private IEnumerator computeFrameAsync;
        private IEnumerator sendFrameAsync;


        enum MessageCode
        {
            HostStart,
            HostEnd,
            FrameUpdate
        }

        public MessageLinkedList messageLinkedList_;
        public int hostUID = -1;
        private int myUID;

        private bool monitorsGenerated = false;

        // Start is called before the first frame update
        void Start()
        {

            Debug.Log(tex_array.IsCreated);
            Debug.Assert(texScript != null);
            Debug.Assert(screen != null);

            MessageController msgCon = gameObject.GetComponent<MessageController>();
            if (msgCon == null)
                msgCon = gameObject.AddComponent<MessageController>();
            msgCon.onHostStarted += HostStarted;
            msgCon.onHostStopped += HostEnded;
            msgCon.onFrameMessage += FrameUpdated;


            uDesktopDuplication.Manager.instance.Reinitialize();
            active = false;
            texScript.enabled = false;
            //uDesktopDuplication.Manager.instance.enabled = false;

            refreshgap = 1 / refreshrate;

            screen_mat = texScript.gameObject.GetComponent<Material>();
            //screen._LocallySetFloatCallback(MessageReceivedCallBack);

            myUID = MainController.peer_id;

            msgArr[0] = myUID;
            screen_mat = screen.GetComponent<Renderer>().materials[0];
            receive_mat = new Material(Shader.Find("Unlit/Texture"));

            Material[] mats = screen.GetComponent<Renderer>().materials;
            mats[0] = receive_mat;
            screen.GetComponent<Renderer>().materials = mats;


            //screen.GetComponent<Renderer>().material = receive_mat;
            texScript.monitor.useGetPixels = true;
            prevTexHash = new Hash128();

            UpdateTexture();

            computeFrameAsync = ComputeFrameAsync();
            //sendFrameAsync = SendFrameAsync();
            messageLinkedList_ = new MessageLinkedList();

            monitorGeneratedBoards = new List<GameObject>();

            //renderTex = new RenderTexture(1, 1, 32);
            //Graphics.SetRenderTarget(renderTex);
            GL.LoadPixelMatrix(0, 1, 1, 0);
        }

        void UpdateTexture()
        {
            var width = texScript.monitor.width / resolutiondownscale_;
            var height = texScript.monitor.height / resolutiondownscale_;

            // TextureFormat.BGRA32 should be set but it causes an error now.
            Destroy(tex_);
            tex_ = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex_.filterMode = FilterMode.Bilinear;
        }

        AsyncGPUReadbackRequest CopyTexture()
        {
            //Debug.LogWarning("In Copy Texture!");
            var width = texScript.monitor.width / resolutiondownscale_;
            var height = texScript.monitor.height / resolutiondownscale_;

            Graphics.ConvertTexture(texScript.monitor.texture, tex_);
            //Debug.LogWarning("Texture Converted!");
            //Debug.LogWarning(tex_array == null);
            //Debug.LogWarning(tex_ == null);
            return AsyncGPUReadback.RequestIntoNativeArray(ref gpu_request_array, tex_, 0, TextureFormat.RGBA32);

        }
        IEnumerator ComputeFrameAsync()
        {

            while (active)
            {
                Debug.Log("Compute Frame Call");
                if (tex_array != null)
                    if (tex_array.IsCreated)
                        try
                        {
                            tex_array.Dispose();
                        }
                        catch
                        {

                        }
                if (gpu_request_array != null)
                    if (gpu_request_array.IsCreated)
                        try
                        {
                            gpu_request_array.Dispose();
                        }
                        catch
                        {

                        }

                if (jpg_array != null)
                    if (jpg_array.IsCreated)
                        try
                        {
                            jpg_array.Dispose();
                        }
                        catch
                        {

                        }

                if (screen.GetComponent<Renderer>().materials[0].mainTexture == null)
                {
                    texScript.monitor.Reinitialize();
                    uDesktopDuplication.Manager.instance.Reinitialize();
                    texScript.monitor.useGetPixels = true;
                }
                else if (Time.time - timer > refreshgap)
                {
                    timer = Time.time;

                    if (tex_ == null)
                        UpdateTexture();
                    else if (texScript.monitor.width != tex_.width ||
                        texScript.monitor.height != tex_.height)
                        UpdateTexture();

                    gpu_request_array = new NativeArray<byte>(tex_.width * tex_.height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);


                    AsyncGPUReadbackRequest request = CopyTexture();

                    while (!request.done)
                    {
                        yield return null;
                    }
                    tex_array = new NativeArray<byte>(tex_.width * tex_.height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    tex_array.CopyFrom(request.GetData<byte>());

                    gpu_request_array.Dispose();
                    //Debug.Log(tex_array.Length);

                    Hash128 newTexHash = Hash128.Compute(tex_array);
                    if (prevTexHash == newTexHash)
                        continue;
                    prevTexHash = newTexHash;
                    byte[] jpg_bytes = null;
                    //Create Job here.
                    //bool loopflag = false;

                    //calculateFrameJobData = new CalculateFrameJob();
                    //calculateFrameJobData.height = texScript.monitor.height / resolutiondownscale_;
                    //calculateFrameJobData.width = texScript.monitor.width / resolutiondownscale_;
                    //calculateFrameJobData.texture_array = tex_array;
                    //calculateFrameJobData.jpg_array = jpg_array;

                    //calculateFrameJobData.Run();

                    jpg_array = ImageConversion.EncodeNativeArrayToJPG<Byte>(tex_array, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, (uint)(texScript.monitor.width / resolutiondownscale_), (uint)(texScript.monitor.height / resolutiondownscale_));

                    //while (!loopflag)
                    //{
                    //if (calculateFrameHandle.IsCompleted)
                    //{
                    //calculateFrameHandle.Complete();
                    //Debug.Log(calculateFrameJobData.jpg_array.Length);

                    //Debug.Log("jpg_bytes: " + jpg_bytes.Length);
                    //
                    //loopflag = true;
                    //}
                    //else
                    //{
                    //    yield return new WaitForEndOfFrame();
                    //}
                    //}
                    jpg_bytes = jpg_array.ToArray();
                    //Debug.Log("jpg_bytes compute length: " + jpg_bytes.Length);
                    //tex_array.Dispose();
                    //jpg_array.Dispose();
                    float[] jpg = ConvertByteArrayIntoFloatArray(jpg_bytes, 0, jpg_bytes.Length);
                    //Debug.Log("jpg compute length: " + jpg.Length);
                    tex_array.Dispose();
                    jpg_array.Dispose();

                    yield return StartCoroutine(SendFrameAsync(jpg, jpg_bytes.Length));
                    continue;
                }
                else
                {
                    yield return new WaitForSeconds(refreshgap - (Time.time - timer));
                    continue;
                }
                yield return null;

            }

        }

        IEnumerator SendFrameAsync(float[] jpg, int jpg_length_bytes)
        {
            //Debug.Log("Awaiting Sending");
            yield return null;
            //Debug.Log("Sending");
            //int messageCount = jpg.Length / FLOAT_MESSAGE_MAX_SIZE;
            //float[] messageCount = new float[] { (jpg.Length / FLOAT_MESSAGE_MAX_SIZE) };
            //float[] messageLength = new float[] { jpg.Length };
            int messageBytesRemaining = jpg_length_bytes;
            int messageCode = 2;
            int messageUID = myUID;
            //int messageLengthSent = 0;
            int sentCount = 0;
            //Debug.Log("Entering While");
            while (messageBytesRemaining > 0 && active)
            {
                //Debug.Log(sentCount);
                //Debug.Log(messageBytesRemaining);
                //Debug.Log(jpg.Length - messageBytesRemaining);

                if (messageBytesRemaining <= MESSAGE_MAX_BYTE_SIZE)
                {
                    //Debug.Log("1 message remains!");
                    //payload = new float[(int)messageLength[0] - messageLengthSent];
                    //float[] payload = new float[Mathf.CeilToInt(messageBytesRemaining / sizeof(float))];
                    float[] message = new float[Mathf.CeilToInt((float)messageBytesRemaining / sizeof(float)) + 3];
                    //Buffer.BlockCopy(jpg, messageLengthSent * 4, payload, 0, ((int)messageLength[0] - messageLengthSent) * 4);
                    Buffer.BlockCopy(jpg, jpg_length_bytes - messageBytesRemaining, message, MESSAGE_HEADER_BYTE_SIZE, messageBytesRemaining);

                    message[0] = messageCode;
                    message[1] = messageUID;
                    message[2] = messageBytesRemaining;
                    screen.SendAndSetClaim(() =>
                    {
                        screen.SendFloatArray(message);
                    });
                    messageBytesRemaining = 0;

                }
                else
                {
                    //Debug.Log("More than 1 message remains!");
                    //float[] payload = new float[FLOAT_MESSAGE_MAX_SIZE];
                    float[] message = new float[MESSAGE_MAX_FLOAT_SIZE + MESSAGE_HEADER_FLOAT_SIZE];
                    Buffer.BlockCopy(jpg, jpg_length_bytes - messageBytesRemaining, message, MESSAGE_HEADER_BYTE_SIZE, MESSAGE_MAX_BYTE_SIZE);

                    message[0] = messageCode;
                    message[1] = messageUID;
                    message[2] = messageBytesRemaining;
                    screen.SendAndSetClaim(() =>
                    {
                        screen.SendFloatArray(message);
                    });

                    messageBytesRemaining -= MESSAGE_MAX_BYTE_SIZE;
                    sentCount += 1;
                    if (sentCount >= BURST_MESSAGE_MAX)
                    {
                        //Debug.Log("Sent Count greater than burst message max");
                        yield return null;
                        //Debug.Log("Yield Return");
                        sentCount = 0;
                    }

                }
                //Debug.Log("While Reset Point Reached");
                //Debug.Break();
            }
            //Debug.Log("Finished Sending");

        }

        private void CreateCalculateData()
        {

        }

        public struct CalculateFrameJob : IJob
        {
            public int width;
            public int height;
            [ReadOnly]
            public NativeArray<Byte> texture_array;
            public NativeArray<Byte> jpg_array;

            public void Execute()
            {
                //jpg_array = ImageConversion.EncodeNativeArrayToJPG<Byte>(texture_array, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, (uint)width, (uint)height);

                Debug.Log(jpg_array.Length);
            }

        }


        void OnDestroy()
        {
            if (tex_array != null)
                if (tex_array.IsCreated)
                {
                    try
                    {
                        tex_array.Dispose();
                    }
                    catch
                    {

                    }
                }
                    
            if (gpu_request_array != null)
                if (gpu_request_array.IsCreated)
                {
                    try
                    {
                        gpu_request_array.Dispose();
                    }
                    catch
                    {

                    }
                }
                    
                    
            if (jpg_array != null)
                if (jpg_array.IsCreated)
                {
                    try
                    {
                        jpg_array.Dispose();
                    }
                    catch
                    {

                    }
                }
                    

        }

        public void ScreenChanged(bool inc)
        {
            //texScript = GetComponent<uDesktopDuplication.Texture>();
            var id = texScript.monitorId;
            var n = uDesktopDuplication.Manager.monitorCount;
            if (inc)
            {
                texScript.monitorId = (id - 1 < 0) ? 0 : (id - 1);
            }
            else
            {
                texScript.monitorId = (id + 1 >= n) ? (n - 1) : (id + 1);
            }
        }

        public void SetScreenID(int val)
        {
            texScript.monitorId = val;
        }


        private void HostStarted(float[] arr)
        {
            int messageUID = (int)arr[1];
            hostUID = messageUID;
            received_img_array = null;
            if (active)
            {
                active = false;
                texScript.enabled = false;
                //uDesktopDuplication.Manager.instance.enabled = false;
                //screen.GetComponent<Renderer>().material = receive_mat;
                
                Material[] mats = screen.GetComponent<Renderer>().materials;
                mats[0] = receive_mat;
                screen.GetComponent<Renderer>().materials = mats;
            }
        }

        private void HostEnded(float[] arr)
        {
            int messageUID = (int)arr[1];
            if (messageUID == hostUID)
            {
                Debug.Log("Ending Host!");
                received_img_array = null;
                hostUID = -1;
                Material[] mats = GetComponent<Renderer>().materials;
                Destroy(mats[0].mainTexture);

                //Destroy(receive_mat.mainTexture);
                Texture2D newTex = new Texture2D(1, 1);
                newTex.SetPixel(0, 0, new Color(0.95f, 0.95f, 0.95f));
                mats[0].mainTexture = newTex;
                GetComponent<Renderer>().materials = mats;
            }
                

        }

        private void FrameUpdated(float[] arr)
        {
            int messageUID = (int)arr[1];
            if (hostUID == messageUID)
            {
                //Debug.Log("HostUID is MessageUID");
                int messageBytesRemaining = (int)arr[2];
                int messageBodyLengthInBytes = (arr.Length - MESSAGE_HEADER_FLOAT_SIZE) * sizeof(float);

                if (received_img_array == null)
                    received_img_array = new byte[messageBytesRemaining];
                //byte[] payload = new byte[];
                //Debug.Log(arr.Length);
                //Debug.Log(messageBytesRemaining);
                //Debug.Log(received_img_array.Length);
                //Debug.Log(received_img_array.Length - messageBytesRemaining);
                if (messageBytesRemaining < messageBodyLengthInBytes)
                {
                    Buffer.BlockCopy(arr,
                        MESSAGE_HEADER_BYTE_SIZE,
                        received_img_array,
                        received_img_array.Length - messageBytesRemaining,
                        messageBytesRemaining);
                }
                else
                {
                    Buffer.BlockCopy(arr,
                        MESSAGE_HEADER_BYTE_SIZE,
                        received_img_array,
                        received_img_array.Length - messageBytesRemaining,
                        messageBodyLengthInBytes);
                }

                //messageLinkedList_.Add(payload, messageBytesRemaining);
                if (messageBytesRemaining - messageBodyLengthInBytes <= 0)
                {
                    Debug.Log("Message Complete");

                    //var bytes = messageLinkedList_.GetCombinedMessage();
                    Destroy(screen.GetComponent<Renderer>().materials[0].mainTexture);
                    Texture2D emptyTex = new Texture2D(1, 1);
                    //Debug.Log("Image Loading");
                    emptyTex.LoadImage(received_img_array);
                    ReceiveTex = emptyTex;
                    screen.GetComponent<Renderer>().materials[0].mainTexture = emptyTex;
                    Debug.Log("Set Texture!");

                    received_img_array = null;
                }
            }


        }
        public void ScreenActivated()
        {
            if (!active)
            {
                float messageCode = (int)MessageController.MessageCode.HostStart;
                float messageUID = myUID;
                float[] message = new float[] { messageCode, messageUID };
                active = true;
                Material[] mats = screen.GetComponent<Renderer>().materials;
                mats[0] = screen_mat;
                screen.GetComponent<Renderer>().materials = mats;
                texScript.enabled = true;
                hostUID = myUID;
                StartCoroutine(computeFrameAsync);
                screen.SendAndSetClaim(() =>
                {
                    screen.SendFloatArray(message);
                });


            }
        }

        public void screenInactived()
        {

            float messageCode = (int)MessageController.MessageCode.HostEnd;
            float messageUID = myUID;
            float[] message = new float[] { messageCode, messageUID };
            Debug.Log("Screen should be inactivated now!");
            active = false;
            texScript.enabled = false;
            StopAllCoroutines();
            Material[] mats = screen.GetComponent<Renderer>().materials;
            mats[0] = receive_mat;
            screen.GetComponent<Renderer>().materials = mats;
            Destroy(receive_mat.mainTexture);
            Texture2D newTex = new Texture2D(1, 1);
            newTex.SetPixel(0, 0, new Color(0.95f, 0.95f, 0.95f));
            receive_mat.mainTexture = newTex;
            screen.SendAndSetClaim(() =>
            {
                screen.SendFloatArray(message);
            });
            //uDesktopDuplication.Manager.instance.enabled = false;

        }

        private static float[] ConvertByteArrayIntoFloatArray(byte[] _payload, int _floatStartLocation, int _floatArraySize)
        {
            float[] floats = new float[Mathf.CeilToInt((float)_floatArraySize / sizeof(float))];

            Buffer.BlockCopy(_payload, _floatStartLocation, floats, 0, _floatArraySize);

            return floats;
        }



        private float[] CombineFloatArrays(float[] _first, float[] _second, float[] _third, float[] _fourth, float[] _fifth)
        {

            float[] combinedResults = new float[_first.Length + _second.Length + _third.Length + _fourth.Length + _fifth.Length];
            int offset = 0;

            Buffer.BlockCopy(_first, 0, combinedResults, offset, _first.Length * 4);

            offset = _first.Length * 4;
            Buffer.BlockCopy(_second, 0, combinedResults, offset, _second.Length * 4);

            offset = (_first.Length + _second.Length) * 4;
            Buffer.BlockCopy(_third, 0, combinedResults, offset, _third.Length * 4);

            offset = (_first.Length + _second.Length + _third.Length) * 4;
            Buffer.BlockCopy(_fourth, 0, combinedResults, offset, _fourth.Length * 4);

            offset = (_first.Length + _second.Length + _third.Length + _fourth.Length) * 4;
            Buffer.BlockCopy(_fifth, 0, combinedResults, offset, _fifth.Length * 4);

            return combinedResults;
        }

        private float[] CombineFloatArrays(float[] _first, float[] _second, float[] _third, float[] _fourth)
        {

            float[] combinedResults = new float[_first.Length + _second.Length + _third.Length + _fourth.Length];
            int offset = 0;

            Buffer.BlockCopy(_first, 0, combinedResults, offset, _first.Length * 4);

            offset = _first.Length * 4;
            Buffer.BlockCopy(_second, 0, combinedResults, offset, _second.Length * 4);

            offset = (_first.Length + _second.Length) * 4;
            Buffer.BlockCopy(_third, 0, combinedResults, offset, _third.Length * 4);

            offset = (_first.Length + _second.Length + _third.Length) * 4;
            Buffer.BlockCopy(_fourth, 0, combinedResults, offset, _fourth.Length * 4);

            return combinedResults;
        }

        public virtual void PointerClicked(HandController.PointerClickValues vals)
        {
            if(vals.pointerHand.currentAttachedObject != null && vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.laserPointer)
            {
                if (!active)
                {
                    if (!monitorsGenerated)
                    {
                        //activeProjector = hit.transform.parent.gameObject;
                        GenerateScreenSharingCanvas();
                        monitorsGenerated = true;
                    }
                }
                else
                {
                    screenInactived();
                }
            }
            
            
        }

        public virtual void PointerClicked()
        {
            if(!active)
            {
                if (MainController.getInstance().clientTypeVR)
                {
                    if (!monitorsGenerated)
                    {
                        //activeProjector = hit.transform.parent.gameObject;
                        GenerateScreenSharingCanvas();
                        monitorsGenerated = true;
                    }
                }
                else
                {
                    MainController.Player.GetComponent<PC_CameraControl>().screenPicker.gameObject.SetActive(true);
                    MainController.Player.GetComponent<PC_CameraControl>().screenPicker.Activate(this);
                }
            } else
            {
                screenInactived();
            }                
            
            
        }

        private void GenerateScreenSharingCanvas()
        {
            if(MainController.getInstance().clientTypeVR)
            {
                // player.bodyDirectionGuess.normalized;
                Valve.VR.InteractionSystem.Player player = IndexPlayerIntereractions.instance.player;
                Vector3 headPos = IndexPlayerIntereractions.instance.player.hmdTransform.position;
                Vector3 headForward = player.bodyDirectionGuess.normalized;
                Vector3 headRight = -Vector3.Cross(headForward, Vector3.up).normalized;
                Vector3 headLeft = Vector3.Cross(headForward, Vector3.up).normalized;

                // each screen is 1.1 units in size total (including .9 units for actual screen, .1 units around each side)

                float screenSize = .55f;
                int numberOfMoniters = uDesktopDuplication.Manager.monitors.Count;
                int monitorsEachSide = numberOfMoniters / 2;
                bool odd = numberOfMoniters % 2 == 1;
                int leftMonitors = monitorsEachSide - (odd ? 0 : 1);
                int rightMonitors = 0;
                float outFromHead = 1.1f;
                for (int i = 0; i < numberOfMoniters; i++)
                {
                    GameObject button = Instantiate(Resources.Load("Prefabs/Monitor Board", typeof(GameObject)) as GameObject);

                    EventTrigger trigger = button.GetComponent<EventTrigger>();
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.Submit;
                    entry.callback.AddListener(MonitorClicked);
                    trigger.triggers.Add(entry);
                    monitorGeneratedBoards.Add(button);

                    button.transform.SetParent(player.transform);
                    button.name = i.ToString();
                    button.GetComponent<uDesktopDuplication.Texture>().monitorId = i;
                    Vector3 pos = new Vector3();
                    button.transform.localScale = new Vector3(.045f, .025f, .025f);
                    // adding monitors to left of middle
                    if (i < monitorsEachSide)
                    {
                        pos = headPos + (headLeft * ((screenSize * leftMonitors + (odd ? 0 : screenSize / 2.0f)) * player.transform.localScale.z)) + (headForward * (outFromHead * player.transform.localScale.x));
                        leftMonitors--;
                    }
                    // adding monitors to the middle and right of middle
                    else
                    {
                        pos = headPos + (headRight * ((screenSize * rightMonitors + (odd ? 0 : screenSize / 2.0f)) * player.transform.localScale.z)) + (headForward * (outFromHead * player.transform.localScale.x));
                        rightMonitors++;
                    }

                    button.transform.rotation = Quaternion.FromToRotation(Vector3.forward, headForward);

                    button.transform.position = pos;
                }
            } else
            {
                MainController.Player.GetComponent<PC_CameraControl>().screenPicker.gameObject.SetActive(true);
                MainController.Player.GetComponent<PC_CameraControl>().screenPicker.Activate(this);
            }
            

        }

        private void MonitorClicked(BaseEventData data)
        {
            int monitor = data.selectedObject.GetComponent<uDesktopDuplication.Texture>().monitorId;
            SetScreenID(monitor);
            ScreenActivated();
            
            foreach (GameObject obj in monitorGeneratedBoards)
            {
                Destroy(obj);
            }

            monitorGeneratedBoards.Clear();

            monitorsGenerated = false;
        }

        public void GripReleased()
        {
            foreach (GameObject obj in monitorGeneratedBoards)
            {
                Destroy(obj);
            }
            monitorGeneratedBoards.Clear();
            monitorsGenerated = false;
        }

        public class MessageLinkedList
        {
            public MessageLinkedNode root;
            private int byteCount = 0;

            public void Add(byte[] arr, int byteCount)
            {
                if (root == null)
                {
                    root = new MessageLinkedNode(arr);
                    this.byteCount = byteCount;
                    return;
                }
                else
                {
                    root.AddLast(arr);
                    return;
                }
            }

            public void Clear()
            {
                if (root != null)
                {
                    root.Clear();
                    root = null;
                    byteCount = 0;
                }
                return;
            }

            public byte[] GetCombinedMessage()
            {
                if (root == null)
                    return null;
                byte[] combinedMessage = new byte[byteCount];
                Debug.Log("Received Message Total Bytes: " + byteCount);
                root.GetCombinedMessage(combinedMessage, byteCount);
                return combinedMessage;
            }

            //public static MessageLinkedList operator + (MessageLinkedList linkedList, byte[] arr)
            //{
            //    linkedList.Add(arr);
            //    return linkedList;
            //}

        }
        public class MessageLinkedNode
        {
            public byte[] msg;
            public MessageLinkedNode next;

            public MessageLinkedNode(byte[] msg_val)
            {
                msg = msg_val;

            }

            public MessageLinkedNode GetNext()
            {
                return next;
            }

            public bool SetNext(MessageLinkedNode nextNode)
            {
                next = nextNode;
                return true;
            }

            public MessageLinkedNode GetLast()
            {
                return null;
            }

            public void AddLast(byte[] msg_arr)
            {
                if (next == null)
                    next = new MessageLinkedNode(msg_arr);
                else
                {
                    next.AddLast(msg_arr);
                }
            }

            public int GetTotalLength()
            {
                if (next != null)
                    return next.GetTotalLength() + msg.Length;
                return msg.Length;
            }

            public void GetCombinedMessage(byte[] dst, int dstReverseOffset)
            {
                if (dstReverseOffset < msg.Length)
                {
                    Buffer.BlockCopy(msg, 0, dst, dst.Length - dstReverseOffset, dstReverseOffset);
                }
                else
                {
                    Buffer.BlockCopy(msg, 0, dst, dst.Length - dstReverseOffset, msg.Length);
                }

                if (next != null)
                    next.GetCombinedMessage(dst, dstReverseOffset - msg.Length);
            }

            public void Clear()
            {
                if (next != null)
                    next.Clear();
                next = null;
                msg = null;
                return;
            }

            
        }
    }
}
