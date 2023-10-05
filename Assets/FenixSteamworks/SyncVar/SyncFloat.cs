using System;
using UnityEngine;
using Steamworks;

namespace FenixSteamworks
{
    public class SyncFloat : MonoBehaviour
    {
        [Header("Values")] [Tooltip("Key can not contain : or _")]
        public string key;

        public float value;
        [Header("Settings")] public EP2PSend sendMode = EP2PSend.k_EP2PSendUnreliable;
        [Tooltip("Sync every tick")] public bool sync = true;

        [Tooltip("If true will only sync if value is different than last tick")]
        public bool compare = true;

        private float valueLastTick;

        private void Start()
        {
            SendValue();
            if (sync)
            {
                TickManager.Singleton.onTick.AddListener(Sync);
            }
        }

        public void Sync(ulong tick)
        {
            if (compare && valueLastTick == value) return;
            SendValue();
            valueLastTick = value;
        }

        public void SendValue()
        {
            //CreateMessage
            string message = key + "_F" + ":" + value;
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