using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField]
    private int _playerIndex;

    [SerializeField]
    private GameObject _readyTextGameObject;

    [SerializeField]
    private PlayerVisual _playerVisual;

    [SerializeField] private Button _kickButton;
    [SerializeField] private TextMeshPro _playerNameText;

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        _kickButton.onClick.AddListener(async () => 
        {
            var playerData = MultiplayerManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
            await UnityLobbyManager.Instance.KickPlayer(playerData.PlayerId.ToString());
            MultiplayerManager.Instance.KickPlayer(playerData.ClientId);
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        MultiplayerManager.Instance.OnPlayerDataListChanged += LobbyManager_OnPlayerDataListChanged;
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;
        bool showKickButton = NetworkManager.Singleton.IsServer;

        if (showKickButton && MultiplayerManager.Instance.IsPlayerIndexConnected(_playerIndex))
        {
            var playerData = MultiplayerManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
            showKickButton = playerData.ClientId != NetworkManager.Singleton.LocalClientId;
        }

        _kickButton.gameObject.SetActive(showKickButton);

        UpdatePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdatePlayer()
    {
        if (MultiplayerManager.Instance.IsPlayerIndexConnected(_playerIndex))
        {
            Show();
            var playerData = MultiplayerManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
            _playerVisual.SetPlayerColor(MultiplayerManager.Instance.GetPlayerColor(playerData.ColorId));
            _readyTextGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.ClientId));
            _playerNameText.text = playerData.PlayerName.ToString();
        } else
        {
            Hide();
        }

    }

    /// <summary>
    /// When player data changes. Can be when a player joins/leaves or modifies their own character.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void LobbyManager_OnPlayerDataListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }
}
