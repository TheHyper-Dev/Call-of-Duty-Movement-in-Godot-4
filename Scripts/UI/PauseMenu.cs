using Godot;
using System;

public partial class PauseMenu : BaseMenu
{
	public static PauseMenu singleton;
	[Export] Button resume, team_select, options, quit;
	public override void _EnterTree()
	{
		singleton = this;
		previous_menu = singleton;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		resume.MouseEntered += BaseMenuManager.MouseHoverSFX;
		resume.ButtonUp += BaseMenuManager.MouseClickSFX;

		team_select.ButtonUp += OpenTeamSelectMenu;
	}
	public void OpenTeamSelectMenu()
	{
		BaseMenuManager.MouseClickSFX();
		TeamSelectMenu.singleton.Active = true;
		TeamSelectMenu.singleton.previous_menu.Hide();
	}

	public override bool Active
	{
		get => Visible;
		set
		{
			if (value)
			{
				Show();
				current = this;
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else
			{
				Hide();
				current = null;
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			switch (Client.usageState)
			{
				case Client.UsageState.Player:

					Player.singleton.look.Active = !value;
					Player.singleton.move.Active = !value;
					Player.singleton.cmd.ResetInput();
					break;
				case Client.UsageState.FreeLook:
					FreeLookCamera.singleton.Active = !value;
					FreeLookCamera.singleton.cmd.ResetInput();
					break;
			}

		}
	}
}
