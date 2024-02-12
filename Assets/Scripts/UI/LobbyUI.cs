using Assets.Scripts.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _quickJoinLobbyButton;
    [SerializeField] private Button _joinByCodeButton;
    [SerializeField] private TMP_InputField _lobbyCodeInputField;
    [SerializeField] private TMP_InputField _userName;
    [SerializeField] private Transform _lobbyContainer;
    [SerializeField] private Transform _lobbyTemplate;

    private void Awake()
    {
        _mainMenuButton.onClick.AddListener(async () => { 
            await UnityLobbyManager.Instance.LeaveLobby();
            LevelLoader.Load(LevelEnum.MainMenuScene); });

        _createLobbyButton.onClick.AddListener(() =>
        {
            LobbyCreateUI.Instance.Show();
        });

        _quickJoinLobbyButton.onClick.AddListener(async () =>
        {
            await UnityLobbyManager.Instance.QuickJoin();
        });

        _joinByCodeButton.onClick.AddListener(async () => await UnityLobbyManager.Instance.JoinByCode(_lobbyCodeInputField.text));
        _lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        _userName.text = MultiplayerManager.Instance.GetPlayerName();
        _userName.onValueChanged.AddListener((string newText) =>
        {
            MultiplayerManager.Instance.SetPlayerName(newText);
        });

        UnityLobbyManager.Instance.OnLobbyListChanged += UnityLobbyManager_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void UpdateLobbyList(List<Lobby> lobbies)
    {
        foreach(Transform child in _lobbyContainer)
        {
            if (child == _lobbyTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach(var lobby in lobbies)
        {
            var lobbyTransform = Instantiate(_lobbyTemplate, _lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListItemUI>().SetLobby(lobby);
        }
    }

    private void UnityLobbyManager_OnLobbyListChanged(object sender, OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.Lobbies);
    }

    private void OnDestroy()
    {
        UnityLobbyManager.Instance.OnLobbyListChanged -= UnityLobbyManager_OnLobbyListChanged;
    }
}
