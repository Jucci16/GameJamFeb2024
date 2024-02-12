using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _createPublicLobbyButton;
    [SerializeField] private Button _createPrivateLobbyButton;
    [SerializeField] private TMP_InputField _lobbyNameInputField;

    public static LobbyCreateUI Instance { get; private set; }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        _closeButton.onClick.AddListener(() => Hide());
        _createPublicLobbyButton.onClick.AddListener(async () => await UnityLobbyManager.Instance.Createlobby(_lobbyNameInputField.text, false));
        _createPrivateLobbyButton.onClick.AddListener(async () => await UnityLobbyManager.Instance.Createlobby(_lobbyNameInputField.text, true));

    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        Hide();
    }
}
