using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayTestSceneManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject _playerPrefab;

    [SerializeField]
    private Camera _spectatorCamera;

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
        MultiplayerManager.Instance.ResetPlayersForNewMatch();
        foreach(var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {   
            var player = Instantiate(_playerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    public void EnableSpectatorCamera(bool enabled = true) {
        _spectatorCamera.enabled = enabled; 
    }

    private void Awake()
    {
        Instance = this;
    }
}
