using System;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace FenixSteamworks.Structs
{
    [Serializable]
    public struct P2PEvent
    {
        public string key;
        [Tooltip("Args: Type, Content, Sender")]
        public UnityEvent<string, string, CSteamID> onMessage;
    }
}