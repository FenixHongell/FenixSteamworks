using System.Collections.Generic;
using FenixSteamworks.Enums;
using FenixSteamworks.Structs;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public class ChatHandler : MonoBehaviour
    {
        public static ChatHandler Instance { get; private set; }
        public List<ChatMessage> Chat;
        
        private void Awake()
        {
            //Singleton logic
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        public void SendChatMessage(string content, MessageChannel messageChannel)
        {
            if (NetworkManager.Instance.isHost)
            {
                MessageHandler.SendMessageWithKey(MessageKeyType.ChatMessageSent, content + ";" + messageChannel, EP2PSend.k_EP2PSendReliable, false);
            }
            else
            {
                MessageHandler.SendSingularMessageWithKey(NetworkManager.Instance.HostID, MessageKeyType.ChatMessageSent, content + ";" + messageChannel, EP2PSend.k_EP2PSendReliable);
            }
        }

        public void AppendChatMessage(RecievedMessage msg, MessageChannel messageChannel)
        {
            Chat.Add(new ChatMessage(msg.sender, msg.content, messageChannel));
        }
    }
}