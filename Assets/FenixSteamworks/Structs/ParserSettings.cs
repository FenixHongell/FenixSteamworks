using System;
using UnityEngine.Events;

namespace FenixSteamworks.Structs
{
    [Serializable]
    public struct ParserSettings
    {
        public bool useCustomMessageSystem;
        public UnityEvent<string> customMessageParser;
    }
}