using System;

namespace FenixSteamworks.Enums
{
    public enum MessageKeyType : ushort

    {
        Sync = 0,
        PlayerMovement = 1,
        PlayerRotation = 2,
        PlayerAction = 3,
        General = 4,
        Relay = 5,
        Scene = 6,
        Broadcast = 7,
        Transform = 8,
    }
}