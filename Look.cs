using System;
using Godot;

[GlobalClass]
public sealed partial class Look : Resource
{
    public PlayerController controller;
    public Node3D node;

    [Export] public bool look_enabled = true;

    [Export]
    public Vector2
    Sensitivity = Vector2.One,
    Rot = Vector2.Zero;

    [Export]
    public float
    height_offset = 1.8f,
    cam_lerp_speed = 40f;

    [Export] public bool lerp_enabled = false;
    public void Init(PlayerController controller)
    {
        this.controller = controller;
        node = (Node3D)controller.GetNode(controller.look_path);
    }
    public void UnhandledInput(InputEvent input)
    {
        if (!look_enabled) return;
        if (input is InputEventMouseMotion mouse_motion)
        {
            Rot -= mouse_motion.Relative * Sensitivity;
            if (Mathf.Abs(Rot.X) >= 360f)
                Rot.X = 0f;
            if (Mathf.Abs(Rot.Y) > 89.5f)
                Rot.Y = Mathf.Sign(Rot.Y) * 89.5f;
        }
    }
    Vector2 smoothedRot = Vector2.Zero;
    [Export(PropertyHint.Range, "0.001, 1")] public float Smoothing = 0.2f; // Min value does the most smoothing, Max Value does the least (or none at all if set to 1f)

    public void Update(in float dt)
    {
        //node.GlobalPosition = node.GlobalPosition.Lerp(new Vector3(controller.Position.X, controller.Position.Y + height_offset, controller.Position.Z), dt * cam_lerp_speed);
        if (!lerp_enabled)
        {
            Vector3 player_vel = controller.move.velocity;

            float Y = controller.GlobalPosition.Y + height_offset + controller.move.current_stance_look_height;
            node.GlobalTranslate(dt * player_vel);

            Vector3 cur_pos = node.GlobalPosition;
            cur_pos.Y = Y;
            node.GlobalPosition = cur_pos;
        }
        else
        {
            Vector3 controller_pos = controller.GlobalPosition;
            Vector3 look_pos = node.GlobalPosition;

            float lerp_speed_delta = dt * cam_lerp_speed;
            float lerp_speed_hor = lerp_speed_delta * controller.move.horSpeed;
            float lerp_speed_Y = lerp_speed_delta * Math.Abs(controller.move.velocity.Y);

            if (lerp_speed_Y < lerp_speed_delta)
                lerp_speed_Y = lerp_speed_delta;

            look_pos.X = Mathf.MoveToward(look_pos.X, controller_pos.X, lerp_speed_hor);
            look_pos.Y = Mathf.MoveToward(look_pos.Y, controller_pos.Y + height_offset + controller.move.current_stance_look_height, lerp_speed_Y); // smooth ascending/descending
            look_pos.Z = Mathf.MoveToward(look_pos.Z, controller_pos.Z, lerp_speed_hor);
            node.GlobalPosition = look_pos;
        }
        smoothedRot.X = LerpAngleDegrees(smoothedRot.X, Rot.X, Smoothing);
        smoothedRot.Y = LerpAngleDegrees(smoothedRot.Y, Rot.Y, Smoothing);

        node.GlobalRotationDegrees = new Vector3(smoothedRot.Y, smoothedRot.X, 0f);

    }
    public static float AngleDifference(float from, float to)
    {
        float num = (to - from) % 360f;
        return 2f * num % 360f - num;
    }
    public static float LerpAngleDegrees(float from, float to, float weight)
    {
        return from + AngleDifference(from, to) * weight;
    }
}
