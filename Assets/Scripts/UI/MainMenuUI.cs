using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    private Button _lobbyButton;

    private void Awake()
    {
        if (NetworkManager.Singleton is not null)
            Destroy(NetworkManager.Singleton.gameObject);

        if (LobbyManager.Instance is not null)
            Destroy(LobbyManager.Instance.gameObject);

        _lobbyButton.onClick.AddListener(OpenLobbyScreen);
    }

    public void OpenLobbyScreen()
    {
        LevelLoader.Load(LevelEnum.LobbyScene);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
