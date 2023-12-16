using Godot;


public sealed partial class GameManager : Node
{
    public static GameManager instance;
    public static Viewport viewport;
    public static Window root;
    public static SceneTree sceneTree;
    public static World3D world;
    public static PhysicsDirectSpaceState3D space_state;
    public const float DegToRad = 0.01745329251994329576923690768489f;


    public override void _EnterTree()
    {
        instance = this;
        SetupScene();
    }
    public static void SetupScene()
    {
        sceneTree = (SceneTree)Godot.Engine.GetMainLoop();
        viewport = instance.GetViewport();
        root = sceneTree.Root;
        world = root.World3D;
        space_state = world.DirectSpaceState;
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
}