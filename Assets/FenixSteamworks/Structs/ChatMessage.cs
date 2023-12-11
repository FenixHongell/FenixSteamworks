using System;
using FenixSteamworks.Enums;
using Steamworks;

namespace FenixSteamworks.Structs
{
    public struct ChatMessage
    {
        public ChatMessage(CSteamID sender, string content, MessageChannel messageChannel)
        {
            Sender = sender;
            Content = content;
            MessageChannel = messageChannel;
            Timestamp = DateTime.Now.ToShortTimeString();
        }

        public CSteamID Sender;
        public string Content;
        public MessageChannel MessageChannel;
        public string Timestamp;
    }
}