using System;
using System.Collections.Generic;
using FenixSteamworks.Enums;
using Steamworks;
using UnityEngine;
using FenixSteamworks.Structs;

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
        
        private ushort _serverTick; 

        public ushort ServerTick
        {
            get => _serverTick;
            set
            {
                _serverTick = value;
                InterpolationTick = (ushort) (value - TicksBetweenPositionUpdates);
            }
        }
        [HideInInspector] public ushort InterpolationTick = 0; //Current iterated tick (not synced)
        private ushort _ticksBetweenPositionUpdates = 2;

        public ushort TicksBetweenPositionUpdates
        {
            get => _ticksBetweenPositionUpdates;
            private set
            {
                _ticksBetweenPositionUpdates = value;
                InterpolationTick = (ushort) (ServerTick - value);
            }
        }
        
        //State
        [HideInInspector] public bool isInLobby; //Network lobby, not game lobby
        [HideInInspector] public bool isHost;
        [HideInInspector] public bool inGame;

        private List<NetworkedTransform> NetworkIdentities = new List<NetworkedTransform>();
        
        #endregion
        
        //Settings
        [Header("Settings")]
        public GameObject localPlayerObject; //Local player container
        public GameObject otherPlayerObject; //Initial object for non-local players
        public string onLeaveScene; //Scene loaded when leaving/disconnecting
        [SerializeField] private ushort tickDivergenceTolerance = 1;
        
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

        //Void to call when the player wants to create a lobby
        public void CreateLobby(ELobbyType eLobbyType, ushort maxConnections)
        {
            SteamMatchmaking.CreateLobby(eLobbyType, maxConnections);
        }

        //Check for new players
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
                    
                    //Setup player
                    NetworkedPlayer otherPlayerNetworkIdentity =
                        Instantiate(otherPlayerObject).GetComponent<NetworkedPlayer>();
                    otherPlayerNetworkIdentity.playerID = userByIndexId;
                    otherPlayerNetworkIdentity.playerName = SteamFriends.GetFriendPersonaName(userByIndexId);
                    
                    //Add player to list
                    OtherPlayers.Add(otherPlayerNetworkIdentity);
                }
            }

            if (isHost) return;

            HostID = SteamMatchmaking.GetLobbyOwner(currentLobby);
        }

        //Check if client is in lobby
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
            //If host left then disconnect
            if (HostID == player)
            {
                LobbyManager.Instance.LeaveLobby();
            }
            else
            {
                //Remove player that disconnected
                Destroy(OtherPlayers.Find(p => p.playerID == player).currentPlayerContainerGameObject);
                RemovePlayer(player);
            }
        }

        private void FixedUpdate()
        {
            //Read incoming packets
            ReadMessages();

            if (inGame)
            {
                ServerTick++;
            }

            if (isHost && ServerTick % 200 == 0)
            {
                SyncTick();
            }
        }

        private void ReadMessages()
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

                    RecievedMessage msg = new RecievedMessage(messageReceivedRaw, senderID);

                    if ((ushort) MessageKeyType.Transform == msg.key)
                    {
                        //No need for relay since all networked transforms have static ids
                        string[] messageParts = msg.content.Split(";");
                        NetworkedTransform correspondingTransform = NetworkIdentities.Find(identity =>
                            identity.networkId == int.Parse(messageParts[0]));
                        correspondingTransform.NewPositionUpdate(msg.tick, MessageHandler.Vector3FromString(messageParts[1]), MessageHandler.QuaternionFromString(messageParts[2]));
                        return;
                    }

                    //Check if message is a tick sync
                    if ((ushort) MessageKeyType.Sync == msg.key)
                    {
                        SetTick(msg.tick);
                        return;
                    }
                    
                    if ((ushort) MessageKeyType.PlayerGameObjectChange == msg.key)
                    {
                        // if client performed action relay to other clients
                        if (isHost)
                        {
                            MessageHandler.SendMessageWithKey(MessageKeyType.PlayerGameObjectChange, msg.content + ";" + msg.sender, EP2PSend.k_EP2PSendReliable, true);
                            OtherPlayers.Find(otherPlayer => otherPlayer.playerID == msg.sender)?.SetCurrentPlayerGameObject(int.Parse(msg.content), false);
                        }
                        else
                        {
                            if (!msg.isRelay)
                            {
                                //if not relay then the host has changed its player character
                                OtherPlayers.Find(otherPlayer => otherPlayer.playerID == msg.sender)?.SetCurrentPlayerGameObject(int.Parse(msg.content), false);
                            }
                            else
                            {
                                //if relay then get the corresponding player
                                OtherPlayers.Find(otherPlayer => otherPlayer.playerID == new CSteamID(ulong.Parse(msg.content.Split(";")[1])))?.SetCurrentPlayerGameObject(int.Parse(msg.content.Split(";")[0]), false);
                            }
                        }

                        return;
                    }

                    if ((ushort) MessageKeyType.ChatMessageSent == msg.key)
                    {
                        string[] contentParts = msg.content.Split(";");
                        
                        // if client performed action relay to other clients
                        if (isHost)
                        {
                            MessageHandler.SendMessageWithKey(MessageKeyType.ChatMessageSent, msg.content + ";" + msg.sender, EP2PSend.k_EP2PSendReliable, true);
                            msg.content = contentParts[0];
                            ChatHandler.Instance.AppendChatMessage(msg, (MessageChannel) ushort.Parse(contentParts[1]));
                        }
                        else
                        {
                            msg.content = contentParts[0];
                            if (!msg.isRelay)
                            {
                                ChatHandler.Instance.AppendChatMessage(msg, (MessageChannel) ushort.Parse(contentParts[1]));
                            }
                            else
                            {
                                msg.sender = new CSteamID(ulong.Parse(contentParts[2]));
                                ChatHandler.Instance.AppendChatMessage(msg, (MessageChannel) ushort.Parse(contentParts[1]));
                            }
                        }

                        return;
                    }

                    if ((ushort) MessageKeyType.Voice == msg.key)
                    {
                        string[] contentParts = msg.content.Split(";");
                        
                        OtherPlayers.Find(op => op.playerID == msg.sender).currentPlayerContainerGameObject.GetComponent<VoiceChatPlayer>().PlayAudioFromSource(Convert.FromBase64String(contentParts[0]), uint.Parse(contentParts[1]));

                        return;
                    }
                    
                    //Find corresponding event and invoke it
                    P2PEvents.Find(p2PEvent => (ushort) p2PEvent.key == msg.key).onMessage?.Invoke(msg);
                }
            }
        }

        //Send tick sync message
        private void SyncTick()
        {
            MessageHandler.SendMessageWithKey(MessageKeyType.Sync, "tick", EP2PSend.k_EP2PSendUnreliable, false);
        }

        //Set tick on client side when syncing
        private void SetTick(ushort serverTick)
        {
            if (Mathf.Abs(ServerTick - serverTick) > tickDivergenceTolerance)
            {
                ServerTick = serverTick;
            }
        }

        public void RegisterNetworkIdentity(NetworkedTransform identity)
        {
            NetworkIdentities.Add(identity);
        }
    }
}