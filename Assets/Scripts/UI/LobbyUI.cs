using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    }

    private void Start()
    {
        _userName.text = MultiplayerManager.Instance.GetPlayerName();
        _userName.onValueChanged.AddListener((string newText) =>
        {
            MultiplayerManager.Instance.SetPlayerName(newText);
        });
    }
}
