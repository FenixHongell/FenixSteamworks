using System;
using System.Collections.Generic;

namespace FenixSteamworks.Structs
{
    [Serializable]
    public struct GlobalActionSettings
    {
        public string globalActionKey;
        public List<P2PEvent> globalActions;
    }
}