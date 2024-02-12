using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI _messageText;

    [SerializeField]
    private Button _closeButton;

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        MultiplayerManager.Instance.OnFailedToJoinGame += MultiplayerManager_OnFailedToJoinGame;
        UnityLobbyManager.Instance.OnCreateLobbyStarted += UnityLobbyManager_OnCreateLobbyStarted;
        UnityLobbyManager.Instance.OnCreateLobbyFailed += UnityLobbyManager_OnCreateLobbyFailed;
        UnityLobbyManager.Instance.OnJoinStarted += UnityLobbyManager_OnJoinLobbyStarted;
        UnityLobbyManager.Instance.OnJoinFailed += UnityLobbyManager_OnJoinLobbyFailed;
        _closeButton.onClick.AddListener(() => Hide());

        Hide();
    }



    private void MultiplayerManager_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Show();

        _messageText.text = NetworkManager.Singleton.DisconnectReason;
        if (string.IsNullOrWhiteSpace(_messageText.text))
        {
            _messageText.text = "Failed to connect";
        }
    }

    private void OnDestroy()
    {
        MultiplayerManager.Instance.OnFailedToJoinGame -= MultiplayerManager_OnFailedToJoinGame;
        UnityLobbyManager.Instance.OnCreateLobbyStarted -= UnityLobbyManager_OnCreateLobbyStarted;
        UnityLobbyManager.Instance.OnCreateLobbyFailed -= UnityLobbyManager_OnCreateLobbyFailed;
    }

    private void UnityLobbyManager_OnCreateLobbyStarted(object sender, EventArgs e)
    {

    }

    private void UnityLobbyManager_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        Show();
        _messageText.text = "Unable to create lobby.";
    }

    private void UnityLobbyManager_OnJoinLobbyFailed(object sender, EventArgs e)
    {
        Show();
        _messageText.text = "Failed to join lobby.";
    }

    private void UnityLobbyManager_OnJoinLobbyStarted(object sender, EventArgs e)
    {
        Show();
        _messageText.text = "Joining Lobby...";
    }
}
