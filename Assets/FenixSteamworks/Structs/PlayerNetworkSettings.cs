using System;
using System.Collections.Generic;

namespace FenixSteamworks.Structs
{
    [Serializable]
    public struct PlayerNetworkSettings
    {
        public string playerMovementKey;
        public string playerRotationKey;
        public string playerActionKey;
        public List<P2PEvent> playerActionEvents;
    }
}