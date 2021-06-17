using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour {
    [SerializeField] private ScrollRect scroller;
    [SerializeField] private RectTransform content;
    [SerializeField] private Text chatLine;
    [SerializeField] private InputField input;

    private readonly List<Action<string>> chatEntryCallbacks = null;

    public static ChatController Instance { get; private set; }

    public ChatController()
    {
        Instance = this;
        this.chatEntryCallbacks = new List<Action<string>>();
    }    

    public void AddEntryCallback(Action<string> cb)
    {
        this.chatEntryCallbacks.Add(cb);
    }

    public void Update()
    {
        Canvas.ForceUpdateCanvases();
        scroller.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();

        if (Input.GetKeyUp(KeyCode.Return)) { SubmitFunction(); }
    }

    private void SubmitFunction()
    {
        foreach (var cb in this.chatEntryCallbacks)
        {
            cb(input.text);
        }

        AddChat(null, input.text, true);
        input.text = "";
    }

    public void AddChat(string user, string text, bool fromSelf = false)
    {
        var newLine = ((GameObject)Instantiate(chatLine.gameObject)).GetComponent<Text>();
        newLine.text = fromSelf ? "(You): " + text : "<" + user + ">: " + text;
        newLine.gameObject.SetActive(true);
        newLine.rectTransform.parent = content;
    }
}
