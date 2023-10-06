using System;
using System.Collections.Generic;

namespace FenixSteamworks.Structs
{
    [Serializable]
    public struct PlayerNetworkSettings
    {
        public string playerKey;
        public string playerMovementType;
        public string playerRotationType;
        public List<P2PEvent> playerActionEvents;
        public bool syncInput;
    }
}