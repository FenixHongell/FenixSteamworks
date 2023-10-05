using System;
using UnityEngine;
using Steamworks;

namespace FenixSteamworks
{
    public class SyncFloat : MonoBehaviour
    {
        [Header("Values")]
        public string key;
        public float value;
        [Header("Settings")] 
        public EP2PSend sendMode = EP2PSend.k_EP2PSendUnreliable;
        [Tooltip("Sync every frame")]
        public bool sync = true;
        [Tooltip("If true will only sync if value is different than last frame")]
        public bool compare = true;

        private float valueLastFrame;

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
            string message = key + ":" + value;
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