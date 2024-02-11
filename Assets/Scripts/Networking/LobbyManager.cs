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
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnCientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }



    public void StartClient()
    {
        OnTryingToJoinGame.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnCientDisconnectCallback;

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

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (var playerData in _playerDataList)
        {
            if (playerData.ClientId == clientId) return playerData;
        }

        return default;
    }

    public int GetPlayerIndexFromClientId(ulong clientId)
    {
        for (var i = 0; i <  _playerDataList.Count; i++)
        {
            if (clientId == _playerDataList[i].ClientId) return i;
        }

        return -1;
    }

    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnCientDisconnectCallback(clientId);
    }

    /// <summary>
    /// Client failed to join game.
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkManager_Client_OnCientDisconnectCallback(ulong obj)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Client disconnected (Quit and went back to main menu).
    /// </summary>
    /// <param name="clientId"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void NetworkManager_Server_OnCientDisconnectCallback(ulong clientId)
    {
        for (var i = 0; i < _playerDataList.Count;i++)
        {
            var playerData = _playerDataList[i];
            if (playerData.ClientId == clientId)
            {
                _playerDataList.RemoveAt(i);
            }
        }
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
        _playerDataList.Add(new PlayerData 
        { 
            ClientId = clientId,
            ColorId = GetFirstUnusedColorId(),
        });
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

    public Color GetPlayerColor(int colorId)
    {
        return _playerColors[colorId];
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership =false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams rpcParams = default)
    {

        if (!IsColorAvailable(colorId)) return;

        var playerDataIndex = GetPlayerIndexFromClientId(rpcParams.Receive.SenderClientId);
        var playerData = _playerDataList[playerDataIndex];
        playerData.ColorId = colorId;
        _playerDataList[playerDataIndex] = playerData;
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (PlayerData playerData in _playerDataList)
        {
            if (playerData.ColorId == colorId) return false;
        }

        return true;
    }

    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < _playerColors.Count; i++)
        {
            if (IsColorAvailable(i)) return i;
        }

        return 0;
    }
}
