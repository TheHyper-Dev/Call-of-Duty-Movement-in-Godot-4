using Godot;
using System;

public sealed partial class TeamSelectMenu : BaseMenu
{
	public static TeamSelectMenu singleton;
	[Export] Button[] TeamButtons;
	public override void _EnterTree()
	{
		singleton = this;
		TeamButtons[0].ButtonDown += OnAlliesButtonDown;
		TeamButtons[1].ButtonDown += OnAxisButtonDown;
		TeamButtons[2].ButtonDown += OnSpectatorButtonDown;
	}
	public void OnAlliesButtonDown()
	{
		Client.ChangeUsageState(Client.UsageState.Player);
		Active = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	public void OnAxisButtonDown()
	{
		Client.ChangeUsageState(Client.UsageState.Player);
		Active = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	public void OnSpectatorButtonDown()
	{
		Client.ChangeUsageState(Client.UsageState.FreeLook);
		Active = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

}
