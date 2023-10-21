using System;
using FenixSteamworks.Structs;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public static class MessageHandler
    {
        public static void SendMessage(string key, string type, string content, EP2PSend sendMode)
        {
            if (content.Contains(":")) throw new Exception("Message can not contain the character ':'");

            //CreateMessage
            string message = key + ":" + type + ":" + content;
          
            SendP2PMessage(message,sendMode);
        }

        public static void SendMessage(string message, EP2PSend sendMode)
        {
            if (message.Contains(":")) throw new Exception("Message can not contain the character ':'");
            SendP2PMessage(message,sendMode);
        }

        public static void SendMessage(string key, string type, bool value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + (value ? "1" : "0");
          
            SendP2PMessage(message,sendMode);
        }

        public static void SendMessage(string key, string type, float value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + value;
          
            SendP2PMessage(message,sendMode);
        }
        
        public static void SendMessage(string key, string type, Quaternion value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + value.ToString();
          
            SendP2PMessage(message,sendMode);
        }
        
        public static void SendMessage(string key, string type, Vector3 value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + value.ToString();
          
            SendP2PMessage(message,sendMode);
        }
        
        public static void SendMessage(string key, string type, Vector2 value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + value.ToString();
          
            SendP2PMessage(message,sendMode);
        }
        
        public static void SendSingularMessage(CSteamID remote, string key, string type, string content, EP2PSend sendMode)
        {
            if (content.Contains(":")) throw new Exception("Message can not contain the character ':'");

            //CreateMessage
            string message = key + ":" + type + ":" + content;
          
            SendSingularP2PMessage(remote, message,sendMode);
        }

        public static void SendSingularMessage(CSteamID remote, string message, EP2PSend sendMode)
        {
            if (message.Contains(":")) throw new Exception("Message can not contain the character ':'");

            SendSingularP2PMessage(remote, message,sendMode);
        }

        public static void SendSingularMessage(CSteamID remote, string key, string type, bool value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + (value ? "1" : "0");
          
            SendSingularP2PMessage(remote, message,sendMode);
        }

        public static void SendSingularMessage(CSteamID remote, string key, string type, float value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + value;
          
            SendSingularP2PMessage(remote, message,sendMode);
        }
        
        public static void SendSingularMessage(CSteamID remote, string key, string type, Quaternion value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + value.ToString();
          
            SendSingularP2PMessage(remote, message,sendMode);
        }
        
        public static void SendSingularMessage(CSteamID remote, string key, string type, Vector3 value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + value.ToString();
          
            SendSingularP2PMessage(remote, message,sendMode);
        }
        
        public static void SendSingularMessage(CSteamID remote, string key, string type, Vector2 value, EP2PSend sendMode)
        {
            //CreateMessage
            string message = key + ":" + type + ":" + value.ToString();
          
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
            
            foreach (Player player in NetworkManager.Instance.OtherPlayers)
            {
                SteamNetworking.SendP2PPacket(player.UserID, bytes, (uint)bytes.Length, sendMode);
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