using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private NetworkVariable<LobbyState> _lobbyState = new NetworkVariable<LobbyState>(LobbyState.CharacterSelect);
    private NetworkList<PlayerData> _playerDataList;

    private const int MAX_PLAYER_COUNT = 5;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _playerDataList = new NetworkList<PlayerData>();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnactionApprovalCallback;
        NetworkManager.Singleton.StartHost();
    }



    public void StartClient()
    {
        OnTryingToJoinGame.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnCientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnCientDisconnectCallback(ulong obj)
    {
        OnFailedToJoinGame.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_ConnactionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name != LevelEnum.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game has already started.";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_COUNT) 
        {
            response.Approved = false;
            response.Reason = "Game is full.";
            return;
        }

        response.Approved = true;
    }
}
