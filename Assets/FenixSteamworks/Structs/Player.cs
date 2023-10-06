using Steamworks;
namespace FenixSteamworks.Structs
{
    public struct Player
    {
        public Player(string name, CSteamID userID, NetworkedPlayer gamePlayer)
        {
            Name = name;
            UserID = userID;
            GamePlayer = gamePlayer;
        }
        
        public string Name { get; }
        public CSteamID UserID { get; }
        public NetworkedPlayer GamePlayer;
    }
}