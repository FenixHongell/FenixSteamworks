using System;
using System.Collections.Generic;
using FenixSteamworks.Enums;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using FenixSteamworks.Structs;
using UnityEngine.Serialization;

namespace FenixSteamworks
{
    public class NetworkManager : MonoBehaviour
    {
        #region HiddenValues
        //Singleton
        public static NetworkManager Instance { get; private set; }
        
        //Callbacks
        protected Callback<P2PSessionRequest_t> _p2PSessionRequestCallback;
        
        //Stored Values
        [HideInInspector] public NetworkedPlayer localPlayer;
        [HideInInspector] public CSteamID currentLobby;
        [HideInInspector] public string networkAddress;
        public CSteamID HostID { get; private set; }
        public List<NetworkedPlayer> OtherPlayers { get; private set; }
        
        //Ticks
        [HideInInspector] public ushort serverTick; //The tick on the server (synced)
        [HideInInspector] public ushort clientTick = 0; //The tick on the client (not synced)
        [HideInInspector] public ushort currentTick = 0; //Current iterated tick (not synced)
        
        //State
        [HideInInspector] public bool isInLobby;
        [HideInInspector] public bool isHost;
        [HideInInspector] public bool inGame;
        
        #endregion
        
        //Settings
        [Header("Settings")]
        public GameObject localPlayerObject;
        public GameObject otherPlayerObject;
        public string OnLeaveScene;
        
        [Header("P2P Messages")]
        public List<P2PEvent> P2PEvents;
        
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
            //Return if steam is closed
            if (!SteamManager.Initialized)
            {
                return;
            }

            //Get local steam user id
            CSteamID localUserID = SteamUser.GetSteamID();

            //Set local player data
            localPlayer = Instantiate(localPlayerObject).GetComponent<NetworkedPlayer>();
            localPlayer.playerName = SteamFriends.GetPersonaName();
            localPlayer.playerID = localUserID;
            
            _p2PSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
        }

        public void CreateLobby(ELobbyType eLobbyType, ushort maxConnections)
        {
            SteamMatchmaking.CreateLobby(eLobbyType, maxConnections);
        }

        //Call in intervals
        public void SetLobbyMembers()
        {
            //Get number of users in lobby
            int usersInLobby = SteamMatchmaking.GetNumLobbyMembers(currentLobby);
            
            //For each user
            for (int userByIndex = 0; userByIndex < usersInLobby; userByIndex++)
            {
                //Get id from index
                CSteamID userByIndexId = SteamMatchmaking.GetLobbyMemberByIndex(currentLobby, userByIndex);

                if (userByIndexId != localPlayer.playerID)
                {
                    //Return if player already exists in list
                    if (OtherPlayers.Find(networkedPlayer =>
                            networkedPlayer.playerID == userByIndexId) != null) return;
                    
                    NetworkedPlayer otherPlayerNetworkIdentity =
                        Instantiate(otherPlayerObject).GetComponent<NetworkedPlayer>();
                    otherPlayerNetworkIdentity.playerID = userByIndexId;
                    otherPlayerNetworkIdentity.playerName = SteamFriends.GetFriendPersonaName(userByIndexId);
                    
                    OtherPlayers.Add(otherPlayerNetworkIdentity);
                }
            }

            if (isHost) return;

            HostID = SteamMatchmaking.GetLobbyOwner(currentLobby);
        }

        private bool ExpectingClient(CSteamID steamID)
        {
            foreach (NetworkedPlayer player in OtherPlayers)
            {
                if (player.playerID == steamID)
                {
                    return true;
                }
            }

            return false;
        }

        void OnP2PSessionRequest(P2PSessionRequest_t request)
        {
            //Get id from user that requested session
            CSteamID clientId = request.m_steamIDRemote;
            if (ExpectingClient(clientId))
            {
                SteamNetworking.AcceptP2PSessionWithUser(clientId);
            }
            else
            {
                Debug.LogWarning("Unexpected session request from " + clientId);
            }
        }

        public void RemoveAllPlayers()
        {
            OtherPlayers = new List<NetworkedPlayer>();
        }

        public void RemovePlayer(CSteamID id)
        {
            OtherPlayers.RemoveAll(player => player.playerID == id);
        }

        public void HandlePlayerLeft(CSteamID player)
        {
            if (HostID == player)
            {
                LobbyManager.Instance.LeaveLobby();
            }
            else
            {
                Destroy(OtherPlayers.Find(p => p.playerID == player).currentPlayerGameObject);
                RemovePlayer(player);
            }
        }

        private void FixedUpdate()
        {
            uint size;

            while (SteamNetworking.IsP2PPacketAvailable(out size))
            {
                //Allocate buffer and needed variables
                var buffer = new byte[size];
                uint bytesRead;
                CSteamID senderID;

                //Read the message into the buffer
                if (SteamNetworking.ReadP2PPacket(buffer, size, out bytesRead, out senderID))
                {
                    //Convert to string
                    char[] chars = new char[bytesRead / sizeof(char)];
                    Buffer.BlockCopy(buffer, 0, chars, 0, chars.Length);

                    //Raw string of received message
                    string messageReceivedRaw = new string(chars, 0, chars.Length);
                    
                    //Split message to get key and content
                    string[] messageReceived = messageReceivedRaw.Split(":");

                    //Convert key to ushort from string
                    ushort key = ushort.Parse(messageReceived[0]);
                    
                    string content = messageReceived[1];
                    
                    //Find corresponding event and invoke it
                    P2PEvents.Find(p2PEvent => (ushort) p2PEvent.key == key).onMessage?.Invoke(content, senderID);
                }
            }

            if (inGame)
            {
                serverTick++;
            }

            if (isHost && serverTick % 200 == 0)
            {
                SyncTick();
            }
        }

        private void SyncTick()
        {
            MessageHandler.SendMessageWithKey(MessageKeyType.Sync, serverTick, EP2PSend.k_EP2PSendUnreliable);
        }
    }
}