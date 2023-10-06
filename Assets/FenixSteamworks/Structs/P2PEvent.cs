using System;
using Steamworks;
using UnityEngine.Events;

namespace FenixSteamworks.Structs
{
    [Serializable]
    public struct P2PEvent
    {
        public string key;
        public UnityEvent<string, string, CSteamID> onMessage;
    }
}