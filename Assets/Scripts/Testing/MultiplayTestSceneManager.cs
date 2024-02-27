using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayTestSceneManager : NetworkBehaviour
{
    private static Vector3 player1SpawnPosition = new Vector3(-25f, 0.5f, 0f);
    private static Vector3 player2SpawnPosition = new Vector3(25, 0.5f, 0f);
    private static Vector3 player3SpawnPosition = new Vector3(0f, 0.5f, -25);
    private static Vector3 player4SpawnPosition = new Vector3(0f, 0.5f, 25);
    private static Vector3 player5SpawnPosition = new Vector3(30f, 0.5f, 30f);
    public static List<Vector3> playerSpawnPositions = new List<Vector3>{
        player1SpawnPosition, 
        player2SpawnPosition, 
        player3SpawnPosition, 
        player4SpawnPosition, 
        player5SpawnPosition
    };

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
