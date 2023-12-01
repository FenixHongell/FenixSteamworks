using System;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public class NetworkedPlayer : MonoBehaviour
    {
        public CSteamID playerID;

        [HideInInspector] public string playerName;
        [HideInInspector] public GameObject currentPlayerGameObject;
        [HideInInspector] public bool isLocalPlayer;

        private void Start()
        {
            if (currentPlayerGameObject == null) currentPlayerGameObject = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
    }

}