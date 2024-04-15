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
    [Export] public Movement.StanceState stanceState = Movement.StanceState.Standing;

    [Export] public bool toggle_stance = true;

    public void Init(Player player)
    {
        this.player = player;
    }
    public void HandleInputs(InputEvent input)
    {
        if (!Active) return;

        bool is_pressed;

        Type input_type = input.GetType();


        if (input_type == typeof(InputEventKey))
        {
            InputEventKey key = (InputEventKey)input;
            Key key_code = key.PhysicalKeycode;
            is_pressed = input.IsPressed();
            player.move.UnhandledInputKey(key, in key_code, in is_pressed, input.IsEcho());
        }
        if (input_type == typeof(InputEventMouseMotion))
        {
            InputEventMouseMotion mouse_motion = (InputEventMouseMotion)input;

            player.look.UnhandledInputMouseMotion(mouse_motion, mouse_motion.Relative);
        }
        else if (input_type == typeof(InputEventMouseButton))
        {
            InputEventMouseButton mouse_button = (InputEventMouseButton)input;
            is_pressed = input.IsPressed();
        }
        else if (input_type == typeof(InputEventJoypadMotion))
        {
            InputEventJoypadMotion joypad_motion = (InputEventJoypadMotion)input;
        }
        else if (input_type == typeof(InputEventJoypadButton))
        {
            InputEventJoypadButton joypad_button = (InputEventJoypadButton)input;

            is_pressed = input.IsPressed();
        }
        else if (input_type == typeof(InputEventScreenDrag))
        {
            InputEventScreenDrag screen_drag = (InputEventScreenDrag)input;
        }
        else if (input_type == typeof(InputEventScreenTouch))
        {
            InputEventScreenTouch screen_touch = (InputEventScreenTouch)input;
            is_pressed = input.IsPressed();
        }
    }
    public void ResetInput()
    {
        Client.input_flags = Client.InputBitFlag.None;
        player.move.move_direction = Vector3.Zero;
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
        prone = "prone",
        use = "use";

        // shoot = "shoot",
        // aim = "ads",
        // previous_wep = "previous_wep",
        // next_wep = "next_wep",

        // use = "use",
        // change_stance = "change_stance",

        // reload = "reload";
    }

    public static Dictionary<StringName, Key[]> keyboard_inputs = new()
    {
        {Mappings.left, new Key[] {Key.A}},
        {Mappings.right, new Key[] {Key.D}},
        {Mappings.forward, new Key[] {Key.W}},
        {Mappings.backward, new Key[] {Key.S}},
        {Mappings.jump, new Key[] {Key.Space}},
        {Mappings.sprint, new Key[] {Key.Shift}},
        {Mappings.crouch, new Key[] {Key.C}},
        {Mappings.prone, new Key[] {Key.Ctrl}},
        {Mappings.use, new Key[] {Key.F, Key.T, Key.H}}
    };
    public static bool is_action(in Key key_code, StringName requested_action)
    {
        foreach (Key item in keyboard_inputs[requested_action])
        {
            if (item == key_code)
            {
                return true;
            }
        }
        return false;
    }
    public static bool is_key(in Key key_code, in Key requested_key)
    {
        return key_code == requested_key;
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