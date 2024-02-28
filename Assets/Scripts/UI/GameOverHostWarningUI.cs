using UnityEngine;
using UnityEngine.UI;

public class GameOverHostWarningUI : MonoBehaviour
{
    public static GameOverHostWarningUI Instance;

    [SerializeField] 
    private Button _leaveButton;
    
    [SerializeField] 
    private Button _backButton;

    private void Awake() {
        Instance = this;
    }

    private void Start()
    {
        Hide();
        _leaveButton.onClick.AddListener(async () => { 
            await UnityLobbyManager.Instance.LeaveLobby();
            LevelLoader.Load(LevelEnum.MainMenuScene); 
        });
        _backButton.onClick.AddListener(() => { 
            Hide();
        });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
