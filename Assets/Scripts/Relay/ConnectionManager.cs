using FishNet.Managing;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Networking.Transport;
using FishNet.Transporting.UTP;
using System.Collections.Generic;
using Eflatun.SceneReference;
using System.Threading.Tasks;
public class ConnectionManager : MonoBehaviour
{
    //Constants
    public const string RELAYJOINKEY = "RelayJoinCode", MAPNAMEKEY = "MapName";


    public static ConnectionManager Instance;
    public NetworkManager networkManager;
    Lobby groupLobby, gameplayLobby;
    Allocation hostedAllocation;
    JoinAllocation joinedAllocation;
    public bool inGame;
    public string lobbyName;
    public int maxPlayers;
    public string currentRelayJoinCode;
    public string currentLobbyJoinCode;
    //TESTING ONLY!
    public List<SceneReference> maps;
    public float heartbeatTime;
    float currentHeartbeatTime;
    private void Awake()
    {
        //Enforce singleton, remove any duplicates.
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public async Task<bool> JoinGameLobbyViaCode(string lobbyJoinCode)
    {
        if (!AuthenticationService.Instance.IsAuthorized)
        {
            Debug.LogError("The player is not authorised! How did you get here, little one?", this);
            return false;
        }
        try
        {
            gameplayLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyJoinCode);
        }
        catch (System.Exception)
        {
            throw;
        }
        try
        {
            string relayJoinCode = gameplayLobby.Data[RELAYJOINKEY].Value;
            joinedAllocation = await Relay.Instance.JoinAllocationAsync(relayJoinCode);
        }
        catch (System.Exception)
        {

            throw;
        }
        try
        {
            networkManager.TransportManager.GetTransport<FishyUnityTransport>().SetClientRelayData(
                joinedAllocation.RelayServer.IpV4,
                (ushort)joinedAllocation.RelayServer.Port,
                joinedAllocation.AllocationIdBytes,
                joinedAllocation.Key,
                joinedAllocation.ConnectionData,
                joinedAllocation.HostConnectionData);
            networkManager.ClientManager.StartConnection();
            inGame = true;
        }
        catch (System.Exception)
        {

            throw;
        }
        return true;
    } 
    public async Task<bool> StartGameLobby()
    {
        //We need to create the relay allocation first, as this is necessary for the lobby data.
        try
        {
            print("Creting allocation");
            hostedAllocation = await Relay.Instance.CreateAllocationAsync(maxPlayers);
        }
        catch (System.Exception)
        {
            throw;
        }
        try
        {
            print("Getting join code");
            currentRelayJoinCode = await Relay.Instance.GetJoinCodeAsync(hostedAllocation.AllocationId);
        }
        catch (System.Exception)
        {
            throw;
        }


        int rand = Random.Range(0, maps.Count);
        string sceneName = maps[rand].Name;
        print($"playing on map: {sceneName}");
        try
        {
            print("Creating lobby");
            CreateLobbyOptions clo = new CreateLobbyOptions()
            {
                Data = new()
                {
                    {RELAYJOINKEY, new(DataObject.VisibilityOptions.Private, currentRelayJoinCode)},
                    {MAPNAMEKEY, new(DataObject.VisibilityOptions.Public, sceneName) }
                }
            };
            lobbyName = $"{AuthenticationService.Instance.PlayerId}_Lobby";
            gameplayLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, clo);
        }
        catch (LobbyServiceException e)
        {
            throw e;
        }
        currentLobbyJoinCode = gameplayLobby.LobbyCode;

        try
        {
            print("joining relay");
            FishyUnityTransport t = networkManager.TransportManager.GetTransport<FishyUnityTransport>();
            t.SetHostRelayData(
                hostedAllocation.RelayServer.IpV4,
                (ushort)hostedAllocation.RelayServer.Port,
                hostedAllocation.AllocationIdBytes,
                hostedAllocation.Key,
                hostedAllocation.ConnectionData);
            if (networkManager.ServerManager.StartConnection())
            {
                networkManager.ClientManager.StartConnection();
                inGame = true;
            }
        }
        catch (System.Exception)
        {

            throw;
        }

        print("loading map: " + sceneName);
        GameManager.Instance.LoadSceneOnNetwork(maps[rand]);
        return true;
    }

    private async void FixedUpdate()
    {
        
        if (gameplayLobby != null && gameplayLobby.HostId == AuthenticationService.Instance.PlayerId && currentHeartbeatTime >= heartbeatTime)
        {
            await Lobbies.Instance.SendHeartbeatPingAsync(gameplayLobby.Id);
            currentHeartbeatTime = 0;
        }
        currentHeartbeatTime += Time.fixedDeltaTime;
    }

    public async void QuitMPGame()
    {
        try
        {
            await Lobbies.Instance.RemovePlayerAsync(gameplayLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception)
        {

            throw;
        }

        networkManager.ServerManager.StopConnection(true);

        gameplayLobby = null;
        hostedAllocation = null;
        joinedAllocation = null;
    }
}
