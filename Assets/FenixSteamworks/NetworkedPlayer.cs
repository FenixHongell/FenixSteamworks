using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public class NetworkedPlayer : MonoBehaviour
    {
        public GameObject lobbyPlayer;
        public GameObject gamePlayer;
        public Transform spawnPoint;
        public GameObject currentPlayerGameObject;
        public CSteamID playerID;
        
        //Synced values

        public float mouseX;

        public float mouseY;
        //0: W, 1: A, 2: S, 3: D
        //Extend as needed
        [HideInInspector]
        public bool[] pressedKeys = new bool[4];

        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void SpawnLobbyPlayer()
        {
            currentPlayerGameObject = Instantiate(lobbyPlayer, spawnPoint.position, spawnPoint.rotation);
            currentPlayerGameObject.name = "Player: " + playerID;
        }

        public void SpawnGamePlayer()
        {
            currentPlayerGameObject = Instantiate(gamePlayer, spawnPoint.position, spawnPoint.rotation);
            currentPlayerGameObject.name = "Player: " + playerID;
        }
    }

}