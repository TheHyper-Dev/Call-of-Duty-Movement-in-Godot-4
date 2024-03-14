using System;
using Godot;


[GlobalClass]
public sealed partial class PlayerUI : Resource
{
    public Player player;

    public TextureRect DotCrosshair;
    public HSlider TacSprintStaminaBar;

    public void Init(Player player)
    {
        this.player = player;
        player.SetNode(out DotCrosshair, DotCrosshairPath);
        player.SetNode(out TacSprintStaminaBar, TacSprintStaminaBarPath);
    }

    [ExportGroup("NodePaths")]
    [Export]
    NodePath
        TacSprintStaminaBarPath,
        DotCrosshairPath;
}
