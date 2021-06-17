using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomConfiguration
{
    public string Skybox_left_id;

    public string Skybox_right_id;

    public string Skybox_front_id;

    public string Skybox_back_id;

    public string Skybox_up_id;

    public string Skybox_down_id;

    public Vector3 Pos;

    public Vector3 Fwd;

    public Vector3 Rotation;

    public float Jump_velocity;

    public float Walk_speed;

    public float Run_speed;

    public float Gravity;

    public RoomConfiguration(string skybox_left_id = "", string skybox_right_id = "", string skybox_front_id = "",
        string skybox_back_id = "", string skybox_up_id = "", string skybox_down_id = "", string pos = "0 0 0",
        string fwd = "0 0 0", string rotation = "0 0 0", float jump_velocity = 5.0f, float walk_speed = 1.8f,
        float run_speed = 5.4f, float gravity = -9.8f)
    {
        this.Skybox_left_id = skybox_left_id;
        this.Skybox_right_id = skybox_right_id;
        this.Skybox_front_id = skybox_front_id;
        this.Skybox_back_id = skybox_back_id;
        this.Skybox_up_id = skybox_up_id;
        this.Skybox_down_id = skybox_down_id;
        this.Pos = ObjectUtils.StringToVector3(pos);
        this.Fwd = ObjectUtils.StringToVector3(fwd);
        this.Rotation = ObjectUtils.StringToVector3(rotation);
        this.Jump_velocity = jump_velocity;
        this.Walk_speed = walk_speed;
        this.Run_speed = run_speed;
        this.Gravity = gravity;
    }
}
