using Godot;
using System;

public sealed partial class GlobalLibrary : Resource
{
    public static readonly GlobalLibrary singleton = (GlobalLibrary)ResourceLoader.Load("res://Scripts/Libraries/GlobalLibrary.res");
    [Export] public PackedScene Player_Prefab;
    [Export] public PackedScene GameMenuManager_Prefab;

    [ExportGroup("UI")]
    [Export]
    public AudioStream
    sfx_ui_hover,
    sfx_ui_click;
}
