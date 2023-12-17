using System;

namespace FenixSteamworks.Enums
{
    public enum MessageKeyType : ushort
    {
        Sync = 0, //Sync ticks
        PlayerAction = 1, //Actions by the player, picking up items, abilities, shooting etc...
        Scene = 2, //Changing scenes
        Broadcast = 3, //Global broadcasts
        Transform = 4, //Item transform
        PlayerGameObjectChange = 5, //Changing player object
        ChatMessageSent = 6, //Sending chat messages
        Voice = 7, //Voice chat
    }
}