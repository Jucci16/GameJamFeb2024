using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField]
    private Button _mainMenuButton;

    [SerializeField]
    private Button _readyButton;

    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _lobbyCodeText;

    private void Awake()
    {
        _mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            LevelLoader.Load(LevelEnum.MainMenuScene);
        });

        _readyButton.onClick.AddListener(() =>
        {
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }

    // Start is called before the first frame update
    private void Start()
    {
        var lobby = UnityLobbyManager.Instance.GetLobby();
        const string lobbyNamePrefix = "Lobby Name: ";
        const string lobbyCodePrefix = "Lobby Code: ";
        _lobbyNameText.text =  lobbyNamePrefix + lobby.Name;
        _lobbyCodeText.text = lobbyCodePrefix + lobby.LobbyCode;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
