using System;
using Godot;

[GlobalClass]
public sealed partial class Look : Resource
{
    public Player player;
    public Node3D node;
    [Export] public bool Active = true;

    [Export] public Vector2 Sensitivity = Vector2.One;
    [Export] public Vector2 Rot = Vector2.Zero;

    [Export] public float height_offset = 1.8f;
    [Export] public float cam_lerp_speed = 15f;
    [Export] public CameraSway cameraSway;
    public void Init(Player player)
    {
        this.player = player;
        player.SetNode(out node, look_path);
        cameraSway = cameraSway == null ? new() : (CameraSway)cameraSway.Duplicate();
        cameraSway.Init(this);
    }
    [Export] public Vector3 direction = Vector3.Zero;

    internal float ladder_rotX;
    public void UnhandledInputMouseMotion(InputEventMouseMotion mouse_motion, in Vector2 relative)
    {
        if (!Active) return;
        Rot -= relative * Sensitivity;

        switch (player.move.moveState)
        {
            case Movement.MoveState.Ladder:
                float limit = ladder_rotX + 180f;
                float other_limit;
                if (limit < 0f)
                {
                    other_limit = 360f - limit;

                    if (Rot.X < limit)
                        Rot.X = limit;
                    else if (Rot.X > other_limit)
                        Rot.X = other_limit;
                }
                else
                {
                    other_limit = limit - 360f;

                    if (Rot.X > limit)
                        Rot.X = limit;
                    else if (Rot.X < other_limit)
                        Rot.X = other_limit;
                }
                break;

            default:
                if (Mathf.Abs(Rot.X) >= 360f)
                    Rot.X = 0f;
                break;
        }

        if (Mathf.Abs(Rot.Y) > 89.5f)
            Rot.Y = Mathf.Sign(Rot.Y) * 89.5f;
    }

    Vector3 FinalRot = Vector3.Zero;
    [Export(PropertyHint.Range, "0.001, 1")] public float Smoothing = 0.2f; // Min value does the most smoothing, Max Value does the least (or none at all if set to 1f)

    public void Update()
    {
        direction = Utilities.DegreesToDirection(in Rot.X, in Rot.Y);
        cameraSway.Update();

        Vector3 look_pos = node.GlobalPosition;

        float lerp_speed_hor = player.move.horSpeed;

        if (lerp_speed_hor < player.move.target_speed)// in case the speed becomes higher than the target speed
            lerp_speed_hor = player.move.target_speed;

        lerp_speed_hor = lerp_speed_hor * cam_lerp_speed * Game.Delta;
        if (lerp_speed_hor > 1f)
            lerp_speed_hor = 1f;

        float lerp_speed_Y = Math.Abs(player.move.velocity.Y);

        if (lerp_speed_Y < player.move.target_speed)
            lerp_speed_Y = player.move.target_speed;

        lerp_speed_Y = lerp_speed_Y * cam_lerp_speed * Game.Delta;
        if (lerp_speed_Y > 1f)
            lerp_speed_Y = 1f;

        look_pos.X = Mathf.Lerp(look_pos.X, player.move.GlobalPosition.X, lerp_speed_hor) + cameraSway.position.X;
        look_pos.Y = Mathf.Lerp(look_pos.Y, player.move.GlobalPosition.Y + height_offset + player.move.current_stance_look_height, lerp_speed_Y) + cameraSway.position.Y; // smooth ascending/descending plus camera sway

        look_pos.Z = Mathf.Lerp(look_pos.Z, player.move.GlobalPosition.Z, lerp_speed_hor);
        node.GlobalPosition = look_pos;

        FinalRot.X = Utilities.LerpAngleDegrees(in FinalRot.X, in Rot.X, in Smoothing) + cameraSway.rotation_degrees.X;
        FinalRot.Y = Utilities.LerpAngleDegrees(in FinalRot.Y, in Rot.Y, in Smoothing) + cameraSway.rotation_degrees.Y;
        FinalRot.Z = cameraSway.rotation_degrees.Y;

        node.GlobalRotationDegrees = new Vector3(FinalRot.Y, FinalRot.X, FinalRot.Z);

    }

    [ExportGroup("NodePaths")]
    [Export] NodePath look_path;
}
