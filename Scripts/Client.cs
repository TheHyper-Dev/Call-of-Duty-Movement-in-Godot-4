using System;

using Godot;

public sealed class Client
{
    [Flags]
    public enum InputBitFlag : int
    {
        None = 0,
        Forward = 1 << 0,   // 0001
        Backward = 1 << 1,  // 0010
        Left = 1 << 2,      // 0100
        Right = 1 << 3,      // 1000
        Jump = 1 << 4,
        Attack = 1 << 5
    }
    public static InputBitFlag input_flags = InputBitFlag.None;

    public enum UsageState
    {
        None,
        Player,
        FreeLook
    }
    public static UsageState usageState = UsageState.None;

    public static void ChangeUsageState(UsageState usageState)
    {

        switch (usageState)
        {
            case UsageState.Player:

                if (Client.usageState == UsageState.FreeLook)
                {
                    Map.singleton.RemoveChild(FreeLookCamera.singleton);
                }
                Map.singleton.AddChild(Player.singleton);

                break;
            case UsageState.FreeLook:

                if (Client.usageState == UsageState.Player)
                {
                    Map.singleton.RemoveChild(Player.singleton);
                }
                Map.singleton.AddChild(FreeLookCamera.singleton);

                break;
        }
        Client.usageState = usageState;
    }


}

