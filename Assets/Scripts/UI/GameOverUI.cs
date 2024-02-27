using System.Collections;
using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [SerializeField]
    private TextMeshProUGUI _gameOverText;

    private void Awake() {
        Instance = this;
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
    }

    public bool isGameOver() {
        return gameObject.activeSelf;
    }
}

public enum GameOverStatus {
    winner,
    loser
}
