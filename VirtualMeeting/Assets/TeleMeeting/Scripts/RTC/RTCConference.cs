using System;
using System.Collections;
using System.Collections.Generic;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using ASL;
using UnityEngine;

public class RTCConference : MonoBehaviour
{
    public static RTCConference Instance { get; private set; }

    ICall calls;

    NetworkConfig netConf;
    private string address;

    private const char CHAT_DELIM = ':';

    void Start()
    {
        Instance = this;
        address = Application.productName + "_RTCConference_" + GameLiftManager.GetInstance().m_LobbyManager.m_GameSessionId;
        GameLiftManager.GetInstance().DestroyLobbyManager();
        netConf = new NetworkConfig();
        netConf.SignalingUrl = "ws://signaling.because-why-not.com/conferenceapp";
        netConf.IsConference = true;

        // The current version doesn't deal well with failed direct connections
        // thus a turn server is used to ensure users can connect
        netConf.IceServers.Add(new IceServer("stun:stun.because-why-not.com:443"));
        SetupCalls();
    }

    private void SetupCalls()
    {
        MediaConfig mediaConf1 = new MediaConfig();

        mediaConf1.Video = false;
        mediaConf1.Audio = true;
        Debug.Log("Call setup");
        calls = UnityCallFactory.Instance.Create(netConf);
        calls.CallEvent += OnCallEvent;
        calls.Configure(mediaConf1);
        calls.SetMute(true);
        
        // Send the chat message over RTC when there is a new outgoing line
        ChatController.Instance?.AddEntryCallback(SendChatRTCMessage);
    }

    private void SendChatRTCMessage(string message)
    {
        Debug.Log("Sending chat message over WebRTC");

        if (calls != null)
        {
            calls.Send(ASL.GameLiftManager.GetInstance().m_Username + CHAT_DELIM + message);
        }
    }

    private void OnCallEvent(object src, CallEventArgs args)
    {
        ICall call = src as ICall;

        if (args.Type == CallEventType.ConfigurationComplete)
        {
            Debug.Log("Call configuration done. Listening on address " + address);

            // ALL connections will call listen. The current conference call version
            // will connect all users that listen to the same address
            // resulting in an N to N / full mesh topology
            call.Listen(address);
        }
        else if (args.Type == CallEventType.CallAccepted)
        {
            Debug.Log("Call accepted");
        }
        else if (args.Type == CallEventType.ConfigurationFailed || args.Type == CallEventType.ListeningFailed)
        {
            Debug.LogError("Call failed");
        }
        else if (args.Type == CallEventType.Message)
        {
            Debug.Log("Got new chat message over WebRTC");
            var res = ((MessageEventArgs)args).Content.Split(new[] { CHAT_DELIM }, 2);
            ChatController.Instance?.AddChat(res[0], res[1]);
        }
    }

    private void OnDestroy()
    {
        if (calls == null)
            return;

        calls.Dispose();
        calls = null;
    }

    void Update()
    {
        if (calls == null)
            return;
        
        calls.Update();
    }

    public void Mute()
    {
        if (calls == null)
            return;

        Debug.Log("SetMute(true)");
        calls.SetMute(true);
    }

    public void UnMute()
    {
        if (calls == null)
            return;

        Debug.Log("SetMute(false)");
        calls.SetMute(false);
    }
}
