using Assets.Scripts.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnityLobbyManager : MonoBehaviour
{
    public static UnityLobbyManager Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnJoinFailed;
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;

    private Lobby _joinedLobby;

    private float _heartbeatTimer;
    private float _listLobbiesTimer;

    private const float HEARTBEAT_TIMER_MAX_SECONDS = 15f;
    private const float LIST_LOBBIES_TIMER_MAX_SECONDS = 3f;

    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        await InitializeUnityAuthentication();
    }

    private void Update()
    {
        HandleHeartbeat();
        HandlePeriodicListLobbies();
    }

    private void HandlePeriodicListLobbies()
    {
        if (SceneManager.GetActiveScene().name != LevelEnum.LobbyScene.ToString()) return;
        if (_joinedLobby is not null || !AuthenticationService.Instance.IsSignedIn) return;

        _listLobbiesTimer -= Time.deltaTime;
        if (_listLobbiesTimer > 0) return;

        _listLobbiesTimer = LIST_LOBBIES_TIMER_MAX_SECONDS;
        ListLobbies();
    }

    private void HandleHeartbeat()
    {
        if (!IsLobbyHost()) return;

        _heartbeatTimer -= Time.deltaTime;
        if (_heartbeatTimer > 0f) return;

        _heartbeatTimer = HEARTBEAT_TIMER_MAX_SECONDS;
        LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
    }

    private bool IsLobbyHost()
    {
        return _joinedLobby is not null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async Task InitializeUnityAuthentication()
    {
        if (UnityServices.State is ServicesInitializationState.Initialized) return;

        var initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10_000).ToString());

        await UnityServices.InitializeAsync(initializationOptions);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task ListLobbies()
    {
        try
        {
            var lobbyOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };

            var response = await LobbyService.Instance.QueryLobbiesAsync(lobbyOptions);
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                Lobbies = response.Results
            });

    }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task Createlobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MultiplayerManager.MaxPlayerCount, new CreateLobbyOptions { IsPrivate = isPrivate });
            MultiplayerManager.Instance.StartHost();
            LevelLoader.LoadNetwork(LevelEnum.CharacterSelectScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task QuickJoin()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            MultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task JoinByCode(string lobbyCode)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            MultiplayerManager.Instance.StartClient();
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async Task JoinByLobbyId(string lobbyId)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            MultiplayerManager.Instance.StartClient();
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public Lobby GetLobby() => _joinedLobby;

    public async Task DeleteLobby()
    {
        if (_joinedLobby is null) return;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
            _joinedLobby = null;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }

    }

    public async Task LeaveLobby()
    {
        if (_joinedLobby is null) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async Task KickPlayer(string playerId)
    {
        if (_joinedLobby is null || !IsLobbyHost()) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }
}
