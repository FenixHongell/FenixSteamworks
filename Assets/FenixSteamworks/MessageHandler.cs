using System;
using FenixSteamworks.Enums;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public static class MessageHandler
    {
        public static void SendMessageWithKey(MessageKeyType key, string content, EP2PSend sendMode, bool isRelay)
        {
            if (content.Contains(":") || content.Contains(";")) throw new Exception("Message can not contain the character ':' or ';'");

            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + content + ":" + isRelay.ToString();
          
            SendP2PMessage(message,sendMode);
        }

        public static void SendMessageWithKey(MessageKeyType key, bool value, EP2PSend sendMode, bool isRelay)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value.ToString() + ":" + isRelay.ToString();
          
            SendP2PMessage(message,sendMode);
        }

        public static void SendMessageWithKey(MessageKeyType key, float value, EP2PSend sendMode, bool isRelay)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value + ":" + isRelay.ToString();
          
            SendP2PMessage(message,sendMode);
        }
        
        public static void SendMessageWithKey(MessageKeyType key, Quaternion value, EP2PSend sendMode, bool isRelay)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value.ToString() + ":" + isRelay.ToString();
          
            SendP2PMessage(message,sendMode);
        }
        
        public static void SendMessageWithKey(MessageKeyType key, Vector3 value, EP2PSend sendMode, bool isRelay)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value.ToString() + ":" + isRelay.ToString();
          
            SendP2PMessage(message,sendMode);
        }
        
        public static void SendMessageWithKey(MessageKeyType key, Vector2 value, EP2PSend sendMode, bool isRelay)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value.ToString() + ":" + isRelay.ToString();
          
            SendP2PMessage(message,sendMode);
        }
        
        public static void SendSingularMessageWithKey(CSteamID remote, MessageKeyType key, string content, EP2PSend sendMode)
        {
            if (content.Contains(":") || content.Contains(";")) throw new Exception("Message can not contain the character ':' or ';'");

            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + content + ":" +
                             false.ToString();
          
            SendSingularP2PMessage(remote, message,sendMode);
        }

        public static void SendSingularMessageWithKey(CSteamID remote, MessageKeyType key, bool value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value.ToString() + ":" + false.ToString();
          
            SendSingularP2PMessage(remote, message,sendMode);
        }

        public static void SendSingularMessageWithKey(CSteamID remote, MessageKeyType key, float value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value + ":" + false.ToString();
          
            SendSingularP2PMessage(remote, message,sendMode);
        }
        
        public static void SendSingularMessageWithKey(CSteamID remote, MessageKeyType key, Quaternion value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value.ToString() + ":" + false.ToString();
          
            SendSingularP2PMessage(remote, message,sendMode);
        }
        
        public static void SendSingularMessageWithKey(CSteamID remote, MessageKeyType key, Vector3 value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value.ToString() + ":" + false.ToString();
          
            SendSingularP2PMessage(remote, message,sendMode);
        }
        
        public static void SendSingularMessageWithKey(CSteamID remote, MessageKeyType key, Vector2 value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = (ushort) key + ":" + NetworkManager.Instance.ServerTick + ":" + value.ToString() + ":" + false.ToString();
          
            SendSingularP2PMessage(remote, message,sendMode);
        }

        private static void SendSingularP2PMessage(CSteamID remote, string message, EP2PSend sendMode)
        {
            //Allocate bytes
            byte[] bytes = new byte[message.Length * sizeof(char)];
            Buffer.BlockCopy(message.ToCharArray(), 0, bytes, 0, bytes.Length);
            
            SteamNetworking.SendP2PPacket(remote, bytes, (uint)bytes.Length, sendMode);
        }

        private static void SendP2PMessage(string message, EP2PSend sendMode)
        {
            //Allocate bytes
            byte[] bytes = new byte[message.Length * sizeof(char)];
            Buffer.BlockCopy(message.ToCharArray(), 0, bytes, 0, bytes.Length);
            
            foreach (NetworkedPlayer player in NetworkManager.Instance.OtherPlayers)
            {
                SteamNetworking.SendP2PPacket(player.playerID, bytes, (uint)bytes.Length, sendMode);
            }
        }
        
        public static Vector3 Vector3FromString(string input)
        {
            input = input.Replace("(", "").Replace(")", "");

            string[] values = input.Split(",");
            
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }

        public static Vector2 Vector2FromString(string input)
        {
            input = input.Replace("(", "").Replace(")", "");
            string[] values = input.Split(",");
            return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
        }

        public static Quaternion QuaternionFromString(string input)
        {
            input = input.Replace("(", "").Replace(")", "");
            string[] values = input.Split(",");
            return new Quaternion(float.Parse(values[0]), float.Parse(values[1]),float.Parse(values[2]), float.Parse(values[3]));
        }
    }
}