using System;
using Godot;


[GlobalClass]
public sealed partial class PlayerUI : Resource
{
    public Player player;
    public TextureRect DotCrosshair;
    public HSlider TacSprintStaminaBar;
    public RichTextLabel ClimbLabel;

    public void Init(Player player)
    {
        this.player = player;
        player.SetNode(out DotCrosshair, DotCrosshairPath);
        player.SetNode(out TacSprintStaminaBar, TacSprintStaminaBarPath);
        player.SetNode(out ClimbLabel, ClimbLabelPath);

        ClimbLabel.Text = Constants.PlayerUI.ClimbHintText;
    }

    [ExportGroup("NodePaths")]
    [Export]
    NodePath
        TacSprintStaminaBarPath,
        DotCrosshairPath,
        ClimbLabelPath;
}