using Godot;
using System;

[GlobalClass]
public sealed partial class Player : CharacterBody3D
{
    [ExportGroup("Components")]
    [Export] public Cmd cmd;

    [ExportSubgroup("Movement")]
    [Export] public Movement move;

    [ExportSubgroup("Look")]
    [Export] public Look look;

    [ExportSubgroup("UI")]
    [Export] public PlayerUI ui;

    public override void _EnterTree()
    {
        // Make sure to either instance the components or make them unique before initializing any of them, otherwise one of the Init() calls might fail due to being dependent of another component
        move = move == null ? new() : (Movement)move.Duplicate();
        look = look == null ? new() : (Look)look.Duplicate();
        ui = ui == null ? new() : (PlayerUI)ui.Duplicate();
        cmd = cmd == null ? new() : (Cmd)cmd.Duplicate();

        move.Init(this);
        look.Init(this);
        ui.Init(this);
        cmd.Init(this);

    }

    public override void _UnhandledInput(InputEvent input)
    {
        cmd.HandleInputs(input);
    }
    public override void _PhysicsProcess(double delta)
    {
        move.PhysicsUpdate();
    }
    public override void _Process(double delta)
    {
        look.Update();
        move.Update();
    }
}
