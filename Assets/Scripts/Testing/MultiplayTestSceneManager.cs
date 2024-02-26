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
        int i = 0;
        foreach(var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {   
            Vector2 spawnPosition = new Vector3(0f, 0.5f, 0f) + new Vector3(1f,0f,1f) * (i * 15);
            Transform player = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            i++;
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}
