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

        public void NewRotation(Quaternion newRot)
        {
            
        }

        public void NewPosition(Vector3 newPos)
        {
            
        }
    }

}