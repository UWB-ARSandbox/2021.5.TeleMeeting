using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageItem : Item
{
    public string ID;

    public Vector3 Pos;

    public Vector3 Fwd;

    public Vector3 Rotation;

    public Vector3 Scale;

    public ImageItem(string id = "", string pos = "0 0 0", string fwd = "0 0 1",
        string rotation = "0 0 0", string scale = "1 1 1")
    {
        this.ID = id;
        this.Pos = ObjectUtils.StringToVector3(pos);
        this.Fwd = ObjectUtils.StringToVector3(fwd);
        this.Rotation = ObjectUtils.StringToVector3(rotation);
        this.Scale = ObjectUtils.StringToVector3(scale);
    }
}
