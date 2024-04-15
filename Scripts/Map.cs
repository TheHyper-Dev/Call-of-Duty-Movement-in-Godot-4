using Godot;
using System;

public partial class Map : WorldEnvironment
{
	public static Map singleton;

	public override void _EnterTree()
	{
		singleton = this;

		Player.singleton = (Player)GameplayLibrary.singleton.PlayerPrefab.Instantiate();
		FreeLookCamera.singleton = (FreeLookCamera)GameplayLibrary.singleton.FreeLookCameraPrefab.Instantiate();
	}

}