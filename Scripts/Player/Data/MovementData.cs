using Godot;

[GlobalClass]
public sealed partial class MovementData : Resource
{
    [ExportGroup("Base Movement")]
    [Export]
    public float
    base_move_speed = 5f,
    base_accel = 10f,
    base_air_accel = 0.3f,
    gravity = 38f,
    base_deaccel = 3f,
    base_friction = 3.5f;


    [ExportGroup("On Foot")]
    
    [ExportSubgroup("Standing")]
    [Export]
    public float
    jump_speed = 11f,
    weak_jump_speed = 5f;

    [ExportSubgroup("Crouching")]
    [Export]
    public float
    move_speed_crouch_mult = 0.5f;

    [ExportSubgroup("Prone")]
    [Export]
    public float
    move_speed_prone_mult = 0.2f;

    [ExportGroup("Swimming")]
    [Export]
    public float
    swim_move_speed_mult = 0.8f;


    [ExportGroup("Climbing")]
    [Export]
    public float
    climb_move_speed_mult = 1f;

    [ExportGroup("Sprinting")]
    [Export]
    public float
    sprint_speed_mult = 1.4f,
    tac_sprint_speed_mult = 1.7f,
    tac_sprint_duration = 3f,
    sprint_accel_mult = 0.7f,
    tac_sprint_accel_mult = 0.5f;

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