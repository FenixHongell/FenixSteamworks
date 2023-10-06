using System;
using Steamworks;
using UnityEngine;
using FenixSteamworks.Structs;

namespace FenixSteamworks.SyncVar
{
    public class SyncVec2 : MonoBehaviour
    {
        [Header("Values")] [Tooltip("Key can not contain : or _")]
        public string key;

        public Vector2 value;
        [Header("Settings")] public EP2PSend sendMode = EP2PSend.k_EP2PSendUnreliable;
        [Tooltip("Sync every tick")] public bool sync = true;

        [Tooltip("If true will only sync if value is different than last tick")]
        public bool compare = true;

        private Vector2 valueLastTick;
        
        private void Start()
        {
            SendValue();
        }

        public void Sync()
        {
            SendValue();
            valueLastTick = value;
        }

        public void SendValue()
        {
            //CreateMessage
            string message = key + "_Vec2" + ":" + value.ToString();
            //Allocate bytes
            byte[] bytes = new byte[message.Length * sizeof(char)];
            Buffer.BlockCopy(message.ToCharArray(), 0, bytes, 0, bytes.Length);
            foreach (Player player in NetworkManager.Instance.OtherPlayers)
            {
                SteamNetworking.SendP2PPacket(player.UserID, bytes, (uint)bytes.Length, sendMode);
            }
        }
        
        private void FixedUpdate()
        {
            if (sync)
            {
                if (compare && valueLastTick == value) return;
                Sync();
            }
        }
    }

}