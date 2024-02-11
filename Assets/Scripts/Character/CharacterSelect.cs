using System;
using System.Collections;
using System.Collections.Generic;
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
        _kickButton.onClick.AddListener(() => 
        {
            var playerData = LobbyManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
            LobbyManager.Instance.KickPlayer(playerData.ClientId);
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.Instance.OnPlayerDataListChanged += LobbyManager_OnPlayerDataListChanged;
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;
        bool showKickButton = NetworkManager.Singleton.IsServer;

        if (showKickButton && LobbyManager.Instance.IsPlayerIndexConnected(_playerIndex))
        {
            var playerData = LobbyManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
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
        if (LobbyManager.Instance.IsPlayerIndexConnected(_playerIndex))
        {
            Show();
            var playerData = LobbyManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
            _playerVisual.SetPlayerColor(LobbyManager.Instance.GetPlayerColor(playerData.ColorId));
            _readyTextGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.ClientId));

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
