using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayTestSceneManager : NetworkBehaviour
{
    [SerializeField]
    private Transform _playerPrefab;

    public static MultiplayTestSceneManager Instance {  get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += CumulativeLoadEventCompleted;
        }
    }

    private void CumulativeLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach(var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform player = Instantiate(_playerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}
