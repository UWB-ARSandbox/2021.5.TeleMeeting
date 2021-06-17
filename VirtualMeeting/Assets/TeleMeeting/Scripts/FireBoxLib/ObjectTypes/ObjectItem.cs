using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectItem : Item
{
    public string ID;

    public Vector3 Pos;

    public Vector3 Fwd;

    public float Vel;

    public Vector3 Rotation;

    public Vector3 Scale;

    public Vector3 Rotate_axis;

    public float Rotate_deg_per_sec;
    
    public bool Visible;
    
    public string OnClick;

    public ObjectItem(string id, string pos = "0 0 0", string fwd = "0 0 1",
    float vel = 0f, string rotation = "0 0 0", string scale = "1 1 1",
    string rotate_axis = "0 1 0", float rotate_deg_per_sec = 0f, bool visible = true,
    string onclick = "")
    {
        this.ID = id;
        this.Pos = ObjectUtils.StringToVector3(pos);
        this.Fwd = ObjectUtils.StringToVector3(fwd);
        this.Vel = vel;
        this.Rotation = ObjectUtils.StringToVector3(rotation);
        this.Scale = ObjectUtils.StringToVector3(scale);
        this.Rotate_axis = ObjectUtils.StringToVector3(rotate_axis);
        this.Rotate_deg_per_sec = rotate_deg_per_sec;
        this.Visible = visible;
        this.OnClick = onclick;
    }
}
