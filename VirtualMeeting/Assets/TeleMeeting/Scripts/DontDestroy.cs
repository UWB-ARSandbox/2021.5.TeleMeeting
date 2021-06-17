using ASL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    private void Awake()
    {
        // Ugly hack to prevent duplicating objects in DontDestroyOnLoad
        // because the default lobby scene takes too long to hijack our normal
        // scene and everything is added twice! Only add after the ASL lobby!
        if (GameLiftManager.GetInstance() != null)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
