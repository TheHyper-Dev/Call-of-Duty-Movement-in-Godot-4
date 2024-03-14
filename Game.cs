using System.Collections.Generic;
using System.IO;
using Godot;


public sealed partial class Game : Control
{
    public static Game instance;
    public static Window root;
    public static SceneTree sceneTree;
    public static World3D world;
    public static PhysicsDirectSpaceState3D space_state;

    public static float Delta, PhysicsDelta;
    public ObjectPool<RenderTimer> render_timer_pool = new();
    public ObjectPool<PhysicsTimer> physics_timer_pool = new();

    PhysicsTimer physicsTimer;
    public override void _EnterTree()
    {
        instance = this;
        sceneTree = (SceneTree)Engine.GetMainLoop();
        root = sceneTree.Root;
        InitPhysics();

        instance.AddChild(RayCaster.instance);
        instance.AddChild(ShapeCaster.instance);

        SetupScene();

        BaseMenuManager.singleton = (GameMenuManager)GlobalLibrary.singleton.GameMenuManager_Prefab.Instantiate(); // should depend on the scene
        instance.AddChild(BaseMenuManager.singleton);



        render_timer_pool.Initialize(RenderTimer.Create, 5);
        physics_timer_pool.Initialize(PhysicsTimer.Create, 5);

        physics_timer_pool.Add(out physicsTimer);
        physicsTimer.is_playing = true;
        physicsTimer.time_left = 50f;

        GD.Print(physicsTimer.is_playing);
        foreach (PhysicsTimer physics_Timer in physics_timer_pool.active_pool)
        {
            GD.Print($"{physics_Timer.is_playing}\n{physics_Timer.time_left}");
        }
    }
    public static void SetupScene()
    {
        world = root.World3D;
        space_state = world.DirectSpaceState;
    }
    public static void InitPhysics(in int tickrate = 64)
    {
        Engine.PhysicsTicksPerSecond = tickrate;
        PhysicsDelta = 1f / tickrate;
    }

    bool paused = false;
    public override void _UnhandledInput(InputEvent input)
    {
        if (input is InputEventKey inputEventKey && !inputEventKey.Echo && inputEventKey.Pressed && inputEventKey.PhysicalKeycode == Key.M)
        {
            if (paused)
            {
                Engine.TimeScale = 0.1f;
                paused = false;
            }
            else
            {
                Engine.TimeScale = 1f;
                paused = true;
            }
        }
    }
    public override void _Process(double delta)
    {
        Delta = (float)delta;

        foreach (RenderTimer renderTimer in render_timer_pool.active_pool)
        {
            if (renderTimer.is_playing)
            {
                renderTimer.RenderTick();
            }
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        foreach (PhysicsTimer physicsTimer in physics_timer_pool.active_pool)
        {
            if (physicsTimer.is_playing)
            {
                physicsTimer.PhysicsTick();
            }
        }
    }

    public static void SpawnPlayer()
    {

    }
}
