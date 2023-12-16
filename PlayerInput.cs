using System;
using Godot;

[GlobalClass]
public partial class PlayerInput : Resource
{
    [Export] public bool jump, attack, aim;
    [Export] public Vector2 move = Vector2.Zero;
    [Export] public Movement.SprintState sprintState = Movement.SprintState.None;
    public static class Mappings // You gotta set the inputs in the Project/Project Settings/InputMap, it's better than using the default inputs imo
    {
        public static readonly StringName // Caching the checked StringNames for performance, the same way Godot caches Signal names 
        left = "left",
        right = "right",
        forward = "forward",
        backward = "backward",
        jump = "jump",
        sprint = "sprint",
        crouch = "crouch",
        change_stance = "change_stance",
        prone = "prone";

        // shoot = "shoot",
        // aim = "ads",
        // previous_wep = "previous_wep",
        // next_wep = "next_wep",

        // use = "use",
        // change_stance = "change_stance",

        // reload = "reload";
    }
}

