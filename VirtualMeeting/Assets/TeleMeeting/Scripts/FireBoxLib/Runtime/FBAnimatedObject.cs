using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBAnimatedObject : MonoBehaviour
{
    public float Velocity;

    public Vector3 Rotate_axis;

    public float Rotate_deg_per_sec;

    private void Awake()
    {
        // The object is static by default and only updated via the JS runtime
        this.Velocity = 0f;
        this.Rotate_axis = Vector3.zero;
        this.Rotate_deg_per_sec = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.Rotate_deg_per_sec != 0f && this.Rotate_axis != Vector3.zero)
            transform.rotation *= Quaternion.AngleAxis(this.Rotate_deg_per_sec * Time.deltaTime, this.Rotate_axis);

        // FIXME
        if (Velocity != 0f)
            transform.Translate(new Vector3(0, 1, 0) * Time.deltaTime * this.Velocity);
    }
}
