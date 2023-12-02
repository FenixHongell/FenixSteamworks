using System;
using FenixSteamworks.Enums;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace FenixSteamworks.Structs
{
    [Serializable]
    public struct P2PEvent
    {
        public MessageKeyType key;
        public UnityEvent<RecievedMessage> onMessage;
    }
}