using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


namespace FenixSteamworks
{
    public struct Player
    {
        public Player(string name, CSteamID userID)
        {
            Name = name;
            UserID = userID;
        }
        
        public string Name { get; }
        public CSteamID UserID { get; }
    }
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
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

        public bool isHost = false;
        public string networkAddress = null;
        public Player LocalPlayer { get; private set; }
        public CSteamID currentLobby;
        public List<Player> OtherPlayers;
        public bool isInLobby = false;

        private void Start()
        {
            if (!SteamManager.Initialized)
            {
                return;
            }

            CSteamID localUserID = SteamUser.GetSteamID();

            LocalPlayer = new Player(SteamFriends.GetPersonaName(), localUserID);
        }

        public void CreateLobby(ELobbyType eLobbyType, ushort maxConnections)
        {
            SteamMatchmaking.CreateLobby(eLobbyType, maxConnections);
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
                    OtherPlayers.Add(new Player(SteamFriends.GetFriendPersonaName(iUserID),iUserID));
                }
            }
        }
    }
}
