using System;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace FenixSteamworks
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance;
        //Callbacks
        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> JoinRequest;
        protected Callback<LobbyEnter_t> LobbyEntered;
        protected Callback<LobbyChatUpdate_t> LobbyChatUpdate;

        //Variables
        private const string HostAddressKey = "HostAddress";

        public UnityEvent<LobbyEnter_t, string> OnLobbyEnteredEvent_Everyone;
        public UnityEvent<LobbyEnter_t> OnLobbyEnteredEvent_Client;
        public UnityEvent<LobbyEnter_t> OnLobbyEnteredEvent_Host;
        public UnityEvent<LobbyChatUpdate_t> OnLobbyChatUpdateEvent;

        private void Awake()
        {
            //Singleton logic
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            } 
            DontDestroyOnLoad(this.gameObject);
        }
        private void Start()
        {
            //Check that steam is open
            if (!SteamManager.Initialized)
            {
                return;
            }

            //Register callbacks
            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            //Return if lobby not created successfully
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                return;
            }

            NetworkManager.Instance.isHost = true;

            //Set HostAddressKey and name of lobby
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey,
                SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name",
                SteamFriends.GetPersonaName().ToString() + "'s Lobby");
        }

        private void OnJoinRequest(GameLobbyJoinRequested_t callback)
        {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (NetworkManager.Instance.isInLobby == false) NetworkManager.Instance.isInLobby = true;

            //Everyone
            NetworkManager.Instance.currentLobby = new CSteamID(callback.m_ulSteamIDLobby);
            OnLobbyEnteredEvent_Everyone.Invoke(callback,
                SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name"));

            //Only client
            if (NetworkManager.Instance.isHost == false)
            {
                //Set the the tick to 2 to compensate for delay
                NetworkManager.Instance.ServerTick = 2;
                
                //Set networkAddress in manager
                NetworkManager.Instance.networkAddress =
                    SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
                
                OnLobbyEnteredEvent_Client.Invoke(callback);
            }
            
            //Only Host
            else if (NetworkManager.Instance.isHost)
            {
                OnLobbyEnteredEvent_Host.Invoke(callback);
            }
            
            NetworkManager.Instance.SetLobbyMembers();
        }

        private void OnLobbyChatUpdate(LobbyChatUpdate_t lobbyChatUpdateT)
        {
            //Check that message is in current lobby
            if (lobbyChatUpdateT.m_ulSteamIDLobby != (ulong) NetworkManager.Instance.currentLobby) return;
            
            //Get player affected by message
            CSteamID player = new CSteamID(lobbyChatUpdateT.m_ulSteamIDUserChanged);
            
            //Deal with disconnects & players leaving
            if (lobbyChatUpdateT.m_rgfChatMemberStateChange ==
                (uint) EChatMemberStateChange.k_EChatMemberStateChangeLeft)
            {
                NetworkManager.Instance.HandlePlayerLeft(player);
            }
            else if (lobbyChatUpdateT.m_rgfChatMemberStateChange ==
                     (uint) EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
            {
                NetworkManager.Instance.HandlePlayerLeft(player);
            }

            OnLobbyChatUpdateEvent.Invoke(lobbyChatUpdateT);
        }

        public void LeaveLobby()
        {
            if (NetworkManager.Instance.isInLobby == false) return;
            
            NetworkManager.Instance.isInLobby = false;
            SteamMatchmaking.LeaveLobby(NetworkManager.Instance.currentLobby);
            //Reset current lobby id
            NetworkManager.Instance.currentLobby = CSteamID.Nil;
            //Remove all players from game
            NetworkManager.Instance.RemoveAllPlayers();
            //Set isHost to false when leaving if you are the host
            if (NetworkManager.Instance.isHost) NetworkManager.Instance.isHost = false;
            
            SceneManager.LoadScene(NetworkManager.Instance.onLeaveScene);
        }
    }
}