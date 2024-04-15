using Godot;
using System;

public abstract partial class BaseMenuManager : Control
{
    public static BaseMenuManager singleton;
    public static AudioStreamPlayer sfx_player_ui = new();

    public override void _EnterTree()
    {
        singleton = this;
        AddChild(sfx_player_ui);
    }

    public static void MouseHoverSFX()
    {
        sfx_player_ui.Stream = GlobalLibrary.singleton.sfx_ui_hover;
        sfx_player_ui.Play();
    }
    public static void MouseClickSFX()
    {
        sfx_player_ui.Stream = GlobalLibrary.singleton.sfx_ui_click;
        sfx_player_ui.Play();
    }
    public sealed override void _UnhandledInput(InputEvent input)
    {
        bool is_pressed = input.IsPressed();
        bool is_echo = input.IsEcho();
        Type input_type = input.GetType();

        if (input_type == typeof(InputEventKey))
        {
            InputEventKey key = (InputEventKey)input;
            Key key_code = key.PhysicalKeycode;
            UnhandledInputKey(in key_code, in is_pressed, in is_echo);
        }
        else if (input_type == typeof(InputEventMouseMotion))
        {
            InputEventMouseMotion mouse_motion = (InputEventMouseMotion)input;
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
    public abstract void UnhandledInputKey(in Key key_code, in bool is_pressed, in bool is_echo);
}
