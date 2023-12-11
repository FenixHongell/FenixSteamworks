using FenixSteamworks.Enums;
using Steamworks;

namespace FenixSteamworks.Structs
{
    public struct RecievedMessage
    {
        public RecievedMessage(string rawMessage, CSteamID _sender)
        {
            //Split message to get key and content
            string[] messageReceived = rawMessage.Split(":");
            

            //Convert key to ushort from string
            key = ushort.Parse(messageReceived[0]);
                    
            tick = tick = ushort.Parse(messageReceived[1]);;
            
            content = messageReceived[2];
            
            isRelay = bool.Parse(messageReceived[3]);

            sender = _sender;
        }
        public ushort key;
        public ushort tick;
        public string content;
        public CSteamID sender;
        public bool isRelay;
    }
}