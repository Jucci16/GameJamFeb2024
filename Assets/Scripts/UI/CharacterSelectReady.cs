using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{

    private Dictionary<ulong, bool> _playerReadyDictionary;

    public static CharacterSelectReady Instance { get; private set; }

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
            LevelLoader.LoadNetwork(LevelEnum.MultiplayTestScene);
        }
    }
}
