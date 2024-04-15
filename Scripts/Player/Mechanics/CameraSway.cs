using System;
using Godot;

[GlobalClass]
public sealed partial class CameraSway : Resource
{
    [Export] public bool Active = true;
    public Look look;

    [Export]
    public float
    sway_speed = 1f,
    walk_sway_speed_mult = 0.5f;

    [Export] public Vector2 position = Vector2.Zero;
    [Export] public Vector3 rotation_degrees = Vector3.Zero;


    float timestamp = 0f;

    public void Init(Look look)
    {
        this.look = look;

    }

    public void UnhandledInputMouseMotion(InputEventMouseMotion mouseMotion)
    {
        if (!Active) return;

    }
    public void Update()
    {
        timestamp += Game.Delta * (look.player.move.target_speed + look.player.move.horSpeed) * walk_sway_speed_mult / look.player.move.target_speed;
        if (timestamp > Math.Tau)
            timestamp = 0f;

        position = Utilities.LissajousCurve(in timestamp) * 0.001f;
        rotation_degrees = new(-position.Y / Utilities.pos_rot_balancer_ratio, position.X / Utilities.pos_rot_balancer_ratio, position.X * 1000f);
    }
}