using System;
using Godot;

[GlobalClass]
public sealed partial class GameplayLibrary : Resource
{
    public static readonly GameplayLibrary singleton = (GameplayLibrary)ResourceLoader.Load("res://Scripts/Libraries/GameplayLibrary.res");

    [Export] public PackedScene PlayerPrefab;
    [Export] public PackedScene FreeLookCameraPrefab;

    [Export(PropertyHint.Layers3DPhysics)] public uint water_collision_mask = 8192u;
    [Export(PropertyHint.Layers3DPhysics)] public uint ladder_collision_mask = 16384u;


}