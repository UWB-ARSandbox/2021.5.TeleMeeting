using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundItem : Item
{
    public string ID;

    public bool Loop;

    public SoundItem(string id, bool loop = false)
    {
        this.ID = id;
        this.Loop = loop;
    }
}
