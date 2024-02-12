using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingToServerUI : MonoBehaviour
{
    private void Start()
    {
        MultiplayerManager.Instance.OnTryingToJoinGame += LobbyManager_OnTryingToJoinGame;
        MultiplayerManager.Instance.OnFailedToJoinGame += LobbyManager_OnFailedToJoinGame;
        Hide();
    }

    private void LobbyManager_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        MultiplayerManager.Instance.OnTryingToJoinGame -= LobbyManager_OnTryingToJoinGame;
        MultiplayerManager.Instance.OnFailedToJoinGame -= LobbyManager_OnFailedToJoinGame;
    }
}
