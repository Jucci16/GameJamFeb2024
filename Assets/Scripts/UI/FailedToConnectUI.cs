using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FailedToConnectUI : MonoBehaviour
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
        LobbyManager.Instance.OnFailedToJoinGame += LobbyManager_OnFailedToJoinGame; 
        _closeButton.onClick.AddListener(() => Hide());

        Hide();
    }

    private void LobbyManager_OnFailedToJoinGame(object sender, EventArgs e)
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
        LobbyManager.Instance.OnFailedToJoinGame -= LobbyManager_OnFailedToJoinGame;
    }
}
