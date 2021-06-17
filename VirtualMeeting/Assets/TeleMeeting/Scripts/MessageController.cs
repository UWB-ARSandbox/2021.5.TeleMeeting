using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
using System;

namespace TeleMeeting
{
    public class MessageController : MonoBehaviour
    {
        public enum MessageCode
        {
            HostStart,
            HostEnd,
            FrameUpdate,
            WhiteBoardMessage
        }
        private int myUID;

        public Action<float[]> onHostStarted;
        public Action<float[]> onHostStopped;
        public Action<float[]> onFrameMessage;
        public Action<float[]> onWhiteBoardMessage;

        // Start is called before the first frame update
        void Start()
        {
            ASLObject hostObject = gameObject.GetComponent<ASLObject>();
            hostObject._LocallySetFloatCallback(MessageReceivedCallBack);
            myUID = MainController.peer_id;
        }

        public void MessageReceivedCallBack(string id, float[] arr)
        {
            //Debug.Log("Message Received");
            int messageCode = (int)arr[0];
            int messageUID = (int)arr[1];
            if (messageUID == myUID)
                return;
            switch ((MessageCode)messageCode)
            {
                case MessageCode.HostStart:
                    Debug.Log("Host Started");
                    onHostStarted?.Invoke(arr);
                    break;
                case MessageCode.HostEnd:
                    Debug.Log("Host Ended");
                    onHostStopped?.Invoke(arr);
                    break;
                case MessageCode.FrameUpdate:
                    Debug.Log("Frame Update Received");
                    onFrameMessage?.Invoke(arr);
                    break;
                case MessageCode.WhiteBoardMessage:
                    Debug.Log("Whiteboard Message Received");
                    onWhiteBoardMessage?.Invoke(arr);
                    break;
            }
        }
    }
}
