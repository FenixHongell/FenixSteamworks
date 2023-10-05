using System;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public class SyncVec2 : MonoBehaviour
    {
        [Header("Values")]
        public string key;
        public Vector2 value;
        [Header("Settings")] 
        public EP2PSend sendMode = EP2PSend.k_EP2PSendUnreliable;
        [Tooltip("Sync every frame")]
        public bool sync = true;
        [Tooltip("If true will only sync if value is different than last frame")]
        public bool compare = true;

        private Vector2 valueLastFrame;

        private void Start()
        {
            SendValue();
        }

        private void Update()
        {
            if (sync)
            {
                if (compare && valueLastFrame == value) return;
                SendValue();
            }
        }

        public void SendValue()
        {
            //CreateMessage
            string message = key + ":" + value.ToString();
            //Allocate bytes
            byte[] bytes = new byte[message.Length * sizeof(char)];
            Buffer.BlockCopy(message.ToCharArray(), 0, bytes, 0, bytes.Length);
            foreach (Player player in NetworkManager.Instance.OtherPlayers)
            {
                SteamNetworking.SendP2PPacket(player.UserID, bytes, (uint)bytes.Length, sendMode);
            }
        }
    }

}