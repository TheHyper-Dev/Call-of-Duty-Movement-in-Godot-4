using System;
using Godot;

public abstract partial class BaseMenu : Control
{
    [Export] public BaseMenu previous_menu;
    public static BaseMenu current;
    public virtual bool Active
    {
        get => Visible;
        set
        {
            if (value)
            {
                Show();
                current = this;
            }
            else
            {
                Hide();
                previous_menu.Hide();
                current = previous_menu;
            }
        }
    }
    public void CloseThisAndRootMenus()
    {
        Hide();
        if (previous_menu != null)
        {
            previous_menu.CloseThisAndRootMenus();
        }
        else
        {
            current = null;
        }
    }

}