using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{

    private Dictionary<ulong, bool> _playerReadyDictionary;

    public static CharacterSelectReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;

    private void Awake()
    {
        Instance = this;
        _playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams rpcParams = default) 
    {
        SetPlayerReadyClientRpc(rpcParams.Receive.SenderClientId);
        _playerReadyDictionary[rpcParams.Receive.SenderClientId] = true;
        bool allClientsReady = true;

        foreach(var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId]) 
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            Task.Run(async () => await UnityLobbyManager.Instance.DeleteLobby());
            LevelLoader.LoadNetwork(LevelEnum.MultiplayTestScene);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        _playerReadyDictionary[clientId] = true;
        OnReadyChanged?.Invoke(this, new EventArgs());
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return _playerReadyDictionary.ContainsKey(clientId) && _playerReadyDictionary[clientId];
    }
}
