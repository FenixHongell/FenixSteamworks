using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
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
        [HideInInspector] public Player LocalPlayer;
        [HideInInspector] public CSteamID currentLobby;
        [HideInInspector] public string networkAddress;
        public CSteamID HostID { get; private set; }
        public List<Player> OtherPlayers { get; private set; }
        
        #endregion
        
        //Settings
        public PlayerNetworkSettings playerNetworkSettings;
        public List<P2PEvent> P2PEvents;
        public GameObject localPlayerObject;
        public GameObject otherPlayerObject;
        public string OnLeaveScene;
        public GlobalActionSettings globalActionSettings;
        
        //State
        public bool isInLobby;
        public bool isHost;
        public bool inGame;

        //Custom Parser
        public bool useStandardMessageSystem = true;
        public UnityEvent<string> customMessageParser;
        
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

            LocalPlayer = new Player(SteamFriends.GetPersonaName(), localUserID,
                localPlayerObject.GetComponent<NetworkedPlayer>());

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
                    GameObject otherPlayerWorldObject =
                        Instantiate(otherPlayerObject, new Vector3(0, 0, 0), Quaternion.identity);
                    otherPlayerWorldObject.GetComponent<NetworkedPlayer>().playerID = iUserID;
                    OtherPlayers.Add(new Player(SteamFriends.GetFriendPersonaName(iUserID), iUserID,
                        otherPlayerWorldObject.GetComponent<NetworkedPlayer>()));
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
                }
            }

            return false;
        }

        void OnP2PSessionRequest(P2PSessionRequest_t request)
        {
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
                    
                    string[] messageReceived = messageReceivedRaw.Split(":");

                    string key = messageReceived[0];
                    
                    string type = messageReceived[1];

                    string content = messageReceived[2];
                    
                    if (globalActionSettings.globalActionKey == key)
                    {
                        globalActionSettings.globalActions.Find(a => a.key == type).onMessage?.Invoke(type,content,senderID);
                    } else if (playerNetworkSettings.playerMovementKey == key)
                    {
                        NetworkedPlayer other = OtherPlayers.Find(client => client.UserID == senderID).GamePlayer;
                        other.NewPosition(Vector3FromString(content));
                    } else if (playerNetworkSettings.playerRotationKey == key)
                    {
                        NetworkedPlayer other = OtherPlayers.Find(client => client.UserID == senderID).GamePlayer;
                        other.NewRotation(QuaternionFromString(content));
                    }
                    else if (playerNetworkSettings.playerActionKey == key)
                    {
                        playerNetworkSettings.playerActionEvents.Find(e => e.key == type).onMessage?.Invoke(type, content, senderID);
                        
                    } else
                    {
                        P2PEvents.Find(e => e.key == key).onMessage?.Invoke(type, content, senderID);
                    }
                }
            }
        }

        public Vector3 Vector3FromString(string input)
        {
            input = input.Replace("(", "").Replace(")", "");

            string[] values = input.Split(",");
            
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }

        public Vector2 Vector2FromString(string input)
        {
            input = input.Replace("(", "").Replace(")", "");
            string[] values = input.Split(",");
            return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
        }

        public Quaternion QuaternionFromString(string input)
        {
            input = input.Replace("(", "").Replace(")", "");
            string[] values = input.Split(",");
            return new Quaternion(float.Parse(values[0]), float.Parse(values[1]),float.Parse(values[2]), float.Parse(values[3]));
        }
    }
}