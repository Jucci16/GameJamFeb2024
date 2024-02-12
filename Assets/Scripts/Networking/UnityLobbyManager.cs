using Assets.Scripts.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
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
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

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

        // Running this method asynchronously crashed the compiled game.
        // There seems to be a bug with this version of Unity where attempting to
        // update a TextMeshPro asset from a thread other than main causes an 
        // access violation. This was fun tracking down. Please don't await this. 
        // Please. Like for real. Please don't. I beg of you. Just don't.
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

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            // Subtract one because the host does not count as a connection.
            var allocation = await RelayService.Instance.CreateAllocationAsync(MultiplayerManager.MaxPlayerCount - 1);
            return allocation;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }

        return default;
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }

        return string.Empty;
    }

    public async Task Createlobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MultiplayerManager.MaxPlayerCount, new CreateLobbyOptions { IsPrivate = isPrivate });

            var allocation = await AllocateRelay();
            var relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, 
                new UpdateLobbyOptions 
                { 
                    Data = new Dictionary<string, DataObject> 
                    { 
                        { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) } 
                    } 
                });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            MultiplayerManager.Instance.StartHost();
            LevelLoader.LoadNetwork(LevelEnum.CharacterSelectScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            var relayJoinCode = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return relayJoinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }

        return default;
    }

    public async Task QuickJoin()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            var lobbyCode = _joinedLobby.Data[KEY_RELAY_JOIN_CODE];
            var joinAllocation = await JoinRelay(lobbyCode.Value);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
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
            var joinAllocation = await JoinRelay(lobbyCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            MultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task JoinByLobbyId(string lobbyId)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            var lobbyCode = _joinedLobby.Data[KEY_RELAY_JOIN_CODE];
            var joinAllocation = await JoinRelay(lobbyCode.Value);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            MultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
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
