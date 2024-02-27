using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : NetworkBehaviour
{
    public static GameOverUI Instance;

    [SerializeField]
    private TextMeshProUGUI _gameOverText;

    [SerializeField] 
    private Button _mainMenuButton;
    
    [SerializeField] 
    private Button _spectateButton;
    
    [SerializeField] 
    private Button _spectatorBackButton;

    private void Awake() {
        Instance = this;
        _mainMenuButton.onClick.AddListener(async () => { 
            if(IsServer) {
                GameOverHostWarningUI.Instance.Show();
            } else {
                await UnityLobbyManager.Instance.LeaveLobby();
                LevelLoader.Load(LevelEnum.MainMenuScene); 
            }
        });
        _spectateButton.onClick.AddListener(() => { 
            Hide();
            _spectatorBackButton.gameObject.SetActive(true);
        });
        _spectatorBackButton.onClick.AddListener(() => { 
            gameObject.SetActive(true);
            _spectatorBackButton.gameObject.SetActive(false);
        });
        _mainMenuButton.gameObject.SetActive(false);
        _spectateButton.gameObject.SetActive(false);
        _spectatorBackButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(GameOverStatus status)
    {
        switch(status) {
            case GameOverStatus.loser:
                _gameOverText.text = "Game Over";
                break;
            case GameOverStatus.winner:
                _gameOverText.text = "You Win!";
                _gameOverText.color = Color.green;
                break;
        }
        gameObject.SetActive(true);
        MatchUIManager.instance.Hide();
        RespawnCountdown.Instance.Hide();
        Invoke("showActionButtons", 1);
    }

    private void showActionButtons() {
        _mainMenuButton.gameObject.SetActive(true);
        _spectateButton.gameObject.SetActive(true);
    }

    public bool isGameOver() {
        return gameObject.activeSelf;
    }
}

public enum GameOverStatus {
    winner,
    loser
}
