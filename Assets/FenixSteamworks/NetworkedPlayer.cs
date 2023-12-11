using System;
using System.Collections.Generic;
using FenixSteamworks.Enums;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public class NetworkedPlayer : MonoBehaviour
    {
        public CSteamID playerID;
        public List<GameObject> playerObjects;

        [HideInInspector] public string playerName; 
        [HideInInspector] public bool isLocalPlayer;

        //Player container
        public GameObject currentPlayerContainerGameObject { get; private set; }
        public GameObject playerObject { get; private set; }
        
        private void Start()
        {
            if (currentPlayerContainerGameObject == null) currentPlayerContainerGameObject = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }

        public void SetCurrentPlayerGameObject(int i, bool isLocal)
        {
            if (!NetworkManager.Instance.isInLobby) return;
            
            if (i > playerObjects.Count - 1)
            {
                Debug.LogError("Index out of bounds at SetCurrentPlayerGameObject: i = " + i + ", max: " + (playerObjects.Count - 1));
                return;
            }
            
            if (playerObject != null) Destroy(playerObject);
            playerObject = Instantiate(playerObjects[i], currentPlayerContainerGameObject.transform);

            if (!isLocal) return;
            
            if (NetworkManager.Instance.isHost)
            {
                MessageHandler.SendMessageWithKey(MessageKeyType.PlayerGameObjectChange, i, EP2PSend.k_EP2PSendReliable, false);
            }
            else
            {
                MessageHandler.SendSingularMessageWithKey(NetworkManager.Instance.HostID, MessageKeyType.PlayerGameObjectChange, i, EP2PSend.k_EP2PSendReliable);
            }
        }
    }

}