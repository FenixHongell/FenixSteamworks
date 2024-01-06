# FenixSteamworks
This project is still in its early stages and should NOT be used in production. Bugs and errors are to be expected.

## License: MIT

## About
FenixSteamworks is a C# wrapper for Steamworks.NET. FenixSteamworks is a package containing multiple easily-configurable scripts for easy prototyping.  

## Features
- Client-Side Prediction
- Steam Lobbies
- Message Types
- Steam Networking
- Host Authority
- Easily editable Scripts
- Voice Chat (Beta)
- In-Game Chat (Beta)

## Getting Started
1. Import Steamworks.NET into your Unity project: `https://github.com/rlabrecque/Steamworks.NET`. 
2. Download the latest FenixSteamworks release and add to your Unity project.
3. Create a gameobject and add the *Network Manager.cs* script.
4. Create a gameobject and add the *LobbyManager.cs* script.
5. Configure these to your liking.
6. Give networked objects the *NetworkedTransform.cs* script.
7. Give player prefabs the *NetworkedPlayer.cs* script.
