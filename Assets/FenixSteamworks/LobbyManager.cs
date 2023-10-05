using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace FenixSteamworks
{
    public class LobbyManager : MonoBehaviour
    {
        //Callbacks
        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> JoinRequest;
        protected Callback<LobbyEnter_t> LobbyEntered;

        //Variables
        public ulong CurrentLobbyID;
        private const string HostAddressKey = "HostAddress";

        public UnityEvent<LobbyEnter_t, string> OnLobbyEnteredEvent_Everyone;
        public UnityEvent<LobbyEnter_t> OnLobbyEnteredEvent_Client;
        public UnityEvent<LobbyEnter_t> OnLobbyEnteredEvent_Host;

        private void Start()
        {
            //Check that steam is open
            if (!SteamManager.Initialized)
            {
                return;
            }

            
            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
        
        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            //Return if lobby not created succesfully
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                return;
            }

            NetworkManager.Instance.isHost = true;

            //Set HostAdressKey and name of lobby
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
            //Everyone
            CurrentLobbyID = callback.m_ulSteamIDLobby;
            NetworkManager.Instance.currentLobby = new CSteamID(callback.m_ulSteamIDLobby);
            OnLobbyEnteredEvent_Everyone.Invoke(callback,
                SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name"));
            
            //Only client
            if (NetworkManager.Instance.isHost == false)
            {
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
    }
}