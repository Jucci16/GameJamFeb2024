using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField]
    private List<Color> _playerColors;

    private NetworkVariable<LobbyState> _lobbyState = new NetworkVariable<LobbyState>(LobbyState.CharacterSelect);
    private NetworkList<PlayerData> _playerDataList;

    private const int MAX_PLAYER_COUNT = 5;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataListChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _playerDataList = new NetworkList<PlayerData>();
        _playerDataList.OnListChanged += PlayerDataList_OnListChanged;
    }



    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnactionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_ClientConnectionCallback;
        NetworkManager.Singleton.StartHost();
    }


    public void StartClient()
    {
        OnTryingToJoinGame.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnCientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < _playerDataList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return _playerDataList[playerIndex];
    }

    /// <summary>
    /// Client disconnected or failed to join game.
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkManager_OnCientDisconnectCallback(ulong obj)
    {
        OnFailedToJoinGame.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// When a player attempts to join. Can be declined if game is already started or if game is already full.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
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

    /// <summary>
    /// New client connected.
    /// </summary>
    /// <param name="clientId"></param>
    private void NetworkManager_ClientConnectionCallback(ulong clientId)
    {
        _playerDataList.Add(new PlayerData { ClientId = clientId });
    }

    /// <summary>
    /// When a player joins/leaves the lobby.
    /// </summary>
    /// <param name="changeEvent"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void PlayerDataList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataListChanged?.Invoke(this, EventArgs.Empty);
    }
}
