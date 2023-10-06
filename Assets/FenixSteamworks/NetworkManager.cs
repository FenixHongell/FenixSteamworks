using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using FenixSteamworks.Structs;
using UnityEngine.SceneManagement;

namespace FenixSteamworks
{
    public class  NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        protected Callback<P2PSessionRequest_t> _p2PSessionRequestCallback;
        public bool isHost = false;
        public string networkAddress = null;
        public Player LocalPlayer;
        public CSteamID currentLobby;
        public List<Player> OtherPlayers { get; private set; }
        public bool isInLobby = false;
        public List<P2PEvent> P2PEvents;
        public bool useStandardMessageSystem = true;
        public UnityEvent<string> customMessageParser;
        public PlayerNetworkSettings playerNetworkSettings;
        public GameObject localPlayerObject;
        public GameObject otherPlayerObject;
        public string OnLeaveScene;
        
        public CSteamID HostID { get; private set; }
        private void Awake()
        {
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
            if (!SteamManager.Initialized)
            {
                return;
            }

            CSteamID localUserID = SteamUser.GetSteamID();

            LocalPlayer = new Player(SteamFriends.GetPersonaName(), localUserID, localPlayerObject.GetComponent<NetworkedPlayer>());
            
            _p2PSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
        }

        public void CreateLobby(ELobbyType eLobbyType, ushort maxConnections)
        {
            SteamMatchmaking.CreateLobby(eLobbyType, maxConnections);
            HostID = LocalPlayer.UserID;
        }
        
        //Call in intervals
        public void SetLobbyMembers()
        {
            int usersInLobby = SteamMatchmaking.GetNumLobbyMembers(currentLobby);

            for (int iUser = 0; iUser < usersInLobby; iUser++)
            {
                CSteamID iUserID = SteamMatchmaking.GetLobbyMemberByIndex(currentLobby, iUser);

                if (iUserID != LocalPlayer.UserID)
                {
                    GameObject otherPlayerWorldObject = Instantiate(otherPlayerObject, new Vector3(0, 0, 0), Quaternion.identity);
                    otherPlayerWorldObject.GetComponent<NetworkedPlayer>().playerID = iUserID;
                    OtherPlayers.Add(new Player(SteamFriends.GetFriendPersonaName(iUserID),iUserID, otherPlayerWorldObject.GetComponent<NetworkedPlayer>()));
                }
            }

            if (isHost) return;

            HostID = SteamMatchmaking.GetLobbyOwner(currentLobby);
        }
        
        private bool ExpectingClient(CSteamID steamID)
        {
            foreach (Player player in OtherPlayers)
            {
                if (player.UserID == steamID)
                {
                    return true;
                };
            }
            
            return false;
        }
        
        void OnP2PSessionRequest(P2PSessionRequest_t request)
        {
            CSteamID clientId = request.m_steamIDRemote;
            if (ExpectingClient(clientId))
            {
                SteamNetworking.AcceptP2PSessionWithUser(clientId);
            } else {
                Debug.LogWarning("Unexpected session request from " + clientId);
            }
        }

        public void RemoveAllPlayers()
        {
            OtherPlayers = new List<Player>();
        }

        public void RemovePlayer(CSteamID id)
        {
            OtherPlayers.RemoveAll(player => player.UserID == id);
        }

        public void HandlePlayerLeft(CSteamID player)
        {
            if (HostID == player)
            {
                LobbyManager.Instance.LeaveLobby();
            }
            else
            {
                Destroy(OtherPlayers.Find(p => p.UserID == player).GamePlayer);
                RemovePlayer(player);
            }
        }
        
        private void FixedUpdate()
        {
            uint size;
            
            while (SteamNetworking.IsP2PPacketAvailable(out size))
            {
                // allocate buffer and needed variables
                var buffer = new byte[size];
                uint bytesRead;
                CSteamID senderID;

                // read the message into the buffer
                if (SteamNetworking.ReadP2PPacket(buffer, size, out bytesRead, out senderID))
                {
                    // convert to string
                    char[] chars = new char[bytesRead / sizeof(char)];
                    Buffer.BlockCopy(buffer, 0, chars, 0, chars.Length);

                    string messageReceivedRaw = new string(chars, 0, chars.Length);

                    if (useStandardMessageSystem == false)
                    {
                        customMessageParser.Invoke(messageReceivedRaw);
                        return;
                    }
                    
                    //x_y:z --> [x, y:z]
                    string[] messageReceived = messageReceivedRaw.Split("_");
                    
                    //[x,y:z] i = 0 --> x
                    string key = messageReceived[0]; 
                    
                    //[x,y:z] i = 1 --> _y:z --> [y, z]
                    messageReceived = messageReceived[1].Split(":");
                    
                    // [y,z] i = 0 --> y
                    string type = messageReceived[0];
                    
                    // [y,z] i = 1 --> z
                    string content = messageReceived[1];

                    if (playerNetworkSettings.playerKey == key)
                    {
                        //TODO Write movement vs input system, sorta done depending on how the leave system works.
                        if (playerNetworkSettings.playerMovementType == type)
                        {
                            //Move player
                        } else if (playerNetworkSettings.playerRotationType == type)
                        {
                            //Rotate player
                        }
                        else
                        {
                            playerNetworkSettings.playerActionEvents.Find(e => e.key == type).onMessage.Invoke(type,content,senderID);
                        }
                        
                        return;
                    }

                    P2PEvents.Find(e => e.key == key).onMessage.Invoke(type, content, senderID);
                }
            }
        }
    }
}
