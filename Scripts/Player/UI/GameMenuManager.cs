using System;
using Godot;


public sealed partial class GameMenuManager : BaseMenuManager
{
    public static new GameMenuManager singleton;



    public sealed override void _EnterTree()
    {
        base._EnterTree();
        singleton = this;
    }
    public sealed override void UnhandledInputKey(in Key key_code, in bool is_pressed, in bool is_echo)
    {
        if (!is_echo && is_pressed)
        {
            if (key_code == Key.Escape)
            {
                if (BaseMenu.current == null)
                {
                    PauseMenu.singleton.Active = true;

                }
                else
                {
                    BaseMenu.current.Active = !BaseMenu.current.Active;
                }
            }
        }
    }

}