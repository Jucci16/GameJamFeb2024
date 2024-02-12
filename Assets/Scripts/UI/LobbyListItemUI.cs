using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyNameText;

    private Lobby _lobby;
    public void SetLobby(Lobby lobby)
    {
        _lobby = lobby;
        _lobbyNameText.text = _lobby.Name;
    }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(async () => 
        {
            await UnityLobbyManager.Instance.JoinByLobbyId(_lobby.Id);
        });
    }
}
