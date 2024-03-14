using System;
using System.Collections.Generic;
using Godot;

[GlobalClass, Tool]

public sealed partial class Cmd : Resource
{
    public Player player;
    [Export] public bool Active = true;

    [Export] public bool jump, attack, aim;
    [Export] public Vector2 move = Vector2.Zero;
    [Export] public Movement.SprintState sprintState = Movement.SprintState.None;

    public void Init(Player player)
    {
        this.player = player;
    }
    public void HandleInputs(InputEvent input)
    {
        if (!Active) return;

        bool is_pressed = input.IsPressed();
        bool is_echo = input.IsEcho();
        Type input_type = input.GetType();

        if (input_type == typeof(InputEventKey))
        {
            InputEventKey key = (InputEventKey)input;
            uint key_code = (uint)key.PhysicalKeycode;
            player.move.UnhandledInputKey(key, in key_code, in is_pressed, in is_echo);
        }
        if (input_type == typeof(InputEventMouseMotion))
        {
            InputEventMouseMotion mouse_motion = (InputEventMouseMotion)input;
            player.look.UnhandledInputMouseMotion(mouse_motion);
        }
        else if (input_type == typeof(InputEventMouseButton))
        {
            InputEventMouseButton mouse_button = (InputEventMouseButton)input;
        }
        else if (input_type == typeof(InputEventJoypadMotion))
        {
            InputEventJoypadMotion joypad_motion = (InputEventJoypadMotion)input;
        }
        else if (input_type == typeof(InputEventJoypadButton))
        {
            InputEventJoypadButton joypad_button = (InputEventJoypadButton)input;
        }
        else if (input_type == typeof(InputEventScreenDrag))
        {
            InputEventScreenDrag screen_drag = (InputEventScreenDrag)input;
        }
        else if (input_type == typeof(InputEventScreenTouch))
        {
            InputEventScreenTouch screen_touch = (InputEventScreenTouch)input;
        }
    }

    public static class Mappings // You gotta set the inputs in here
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

    public static Dictionary<StringName, uint[]> keyboard_inputs = new()
    {
        {Mappings.left, new uint[] {(uint)Key.A}},
        {Mappings.right, new uint[] {(uint)Key.D}},
        {Mappings.forward, new uint[] {(uint)Key.W}},
        {Mappings.backward, new uint[] {(uint)Key.S}},
        {Mappings.jump, new uint[] {(uint)Key.Space}},
        {Mappings.sprint, new uint[] {(uint)Key.Shift}},
        {Mappings.crouch, new uint[] {(uint)Key.C}},
        {Mappings.prone, new uint[] {(uint)Key.Ctrl}},
    };
    public static bool is_action(in uint key_code, StringName requested_action)
    {
        foreach (uint item in keyboard_inputs[requested_action])
        {
            if (item == key_code)
            {
                return true;
            }
        }
        return false;
    }

    // [Export] Godot.Collections.Dictionary<uint, StringName> keyboard_inputs_editor
    // {
    //     get 
    //     {
    //         Godot.Collections.Dictionary<uint, StringName> dict = new();
    //         foreach (var item in keyboard_inputs)
    //         {
    //             dict.Add(item.Key, item.Value);
    //         }
    //         return dict;
    //     }
    //     set
    //     {

    //     }
    // }
}
