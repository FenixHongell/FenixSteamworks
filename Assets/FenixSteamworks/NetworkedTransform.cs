using System;
using FenixSteamworks.Enums;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    [RequireComponent(typeof(Interpolator))]
    public class NetworkedTransform : MonoBehaviour
    {
        public int networkId { get; private set; }

        public bool sync = true;
        public EP2PSend sendMode;


        [HideInInspector] public Vector3 currentPosition;
        [HideInInspector] public Quaternion currentRotation;
        [HideInInspector] public Interpolator interpolator;


        private void Awake()
        {
            networkId = this.gameObject.GetInstanceID();
            interpolator = this.gameObject.GetComponent<Interpolator>();
        }

        private void Start()
        {
            NetworkManager.Instance.RegisterNetworkIdentity(this);
        }

        private void FixedUpdate()
        {
            if (NetworkManager.Instance.ServerTick % 2 != 0) return;
            SyncTransform();
        }

        public void SyncTransform()
        {
            if (sync)
            {
                if (NetworkManager.Instance.isHost)
                {
                    MessageHandler.SendMessageWithKey(MessageKeyType.Transform,  networkId + ";" + currentPosition + ";" + currentRotation, sendMode);
                }
                else
                {
                    MessageHandler.SendSingularMessageWithKey(NetworkManager.Instance.HostID,MessageKeyType.Transform, networkId + ";" + currentPosition + ";" + currentRotation, sendMode);
                }
                
            }
        }

        public void NewPositionUpdate(ushort tick, Vector3 position, Quaternion rotation)
        {
            interpolator.NewUpdate(tick, position, rotation);
        }
    }
}