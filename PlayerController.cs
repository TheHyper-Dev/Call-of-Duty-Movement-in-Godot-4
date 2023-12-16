using Godot;
using System;

[GlobalClass]
public sealed partial class PlayerController : CharacterBody3D
{
    [ExportGroup("Components")]
    [Export] public PlayerInput PlayerInput;

    [ExportSubgroup("Movement")]
    [Export] public Movement move;
    [Export] public MovementData move_data;

    [ExportSubgroup("Look")]
    [Export] public Look look;

    [ExportGroup("NodePaths")]

    [Export]
    public NodePath
    world_collider_path,
    look_path;
    public override void _EnterTree()
    {
        // Make sure to either instance the components or make them unique before initializing any of them, otherwise one of the Init() calls might fail due to being dependent of another component
        move = move == null ? new() : (Movement)move.Duplicate();
        look = look == null ? new() : (Look)look.Duplicate();
        PlayerInput = PlayerInput == null ? new() : (PlayerInput)PlayerInput.Duplicate();
        move_data ??= new();

        move.Init(this);
        look.Init(this);
    }
    public override void _UnhandledInput(InputEvent input)
    {
        move.UnhandledInput(input);
        look.UnhandledInput(input);
    }
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        move.PhysicsUpdate(in dt);
    }
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        look.Update(in dt);
        move.Update(in dt);
    }
}