using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNameManager : NetworkBehaviour
{
    public event Action<NetworkConnection, string> OnNameAdded;
    public event Action<NetworkConnection, string> OnNameUpdated;
    public event Action<NetworkConnection> OnNameRemoved;
    
    
    private readonly SyncDictionary<NetworkConnection, string> _names = new();
    public IReadOnlyDictionary<NetworkConnection, string> Names => _names;

    private void Awake()
    {
        _names.OnChange += _names_OnChange;
    }

    private void _names_OnChange(SyncDictionaryOperation op, NetworkConnection key, string value, bool asServer)
    {
        if (op == SyncDictionaryOperation.Add)
            OnNameAdded?.Invoke(key, value);
        else if (op == SyncDictionaryOperation.Set)
            OnNameUpdated?.Invoke(key, value);
        else if (op == SyncDictionaryOperation.Remove)
            OnNameRemoved?.Invoke(key);

    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        NetworkManager.RegisterInstance(this);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
    }
    private void ServerManager_OnRemoteConnectionState(NetworkConnection arg1, FishNet.Transporting.RemoteConnectionStateArgs arg2)
    {
        if(arg2.ConnectionState == FishNet.Transporting.RemoteConnectionState.Stopped)
        {
            _names.Remove(arg1);
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
    }
    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        NetworkManager.UnregisterInstance<PlayerNameManager>();
    }


    public string GetPlayerName(NetworkConnection connection)
    {
        if (_names.TryGetValue(connection, out string result))
            return result;
        return "UNSET";
    }

    [ServerRpc]
    public void SetName(string value, NetworkConnection caller = null)
    {
        _names[caller] = value;
    }
}
