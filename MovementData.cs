using Godot;

[GlobalClass]
public sealed partial class MovementData : Resource
{
    [ExportGroup("Movement")]
    [Export]
    public float
    move_speed_base = 5f,
    move_speed_crouch_mult = 0.5f,
    move_speed_prone_mult = 0.2f,
    sprint_speed_mult = 1.4f,
    tac_sprint_speed_mult = 1.7f,
    tac_sprint_duration = 3f,
    accel = 10f,
    sprint_accel_mult = 0.7f,
    tac_sprint_accel_mult = 0.5f,
    air_accel = 0.3f,
    jump_speed = 11f,
    gravity = 38f,
    deaccel = 3f,
    friction = 3.5f;

    [ExportGroup("FootSteps")]
    [Export]
    public AudioStream[]
    sfx_footstep_metal,
    sfx_footstep_concrete,
    sfx_footstep_dirt,
    sfx_footstep_wood;

    [Export]
    public float
    footstep_walk_duration = 0.4f;

}