using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestLobbyUI : MonoBehaviour
{
    [SerializeField]
    private Button _createGameBtn;

    [SerializeField]
    private Button _joinGameBtn;

    private void Awake()
    {
        _createGameBtn.onClick.AddListener(() =>
        { 
            LobbyManager.Instance.StartHost();
            LevelLoader.LoadNetwork(LevelEnum.CharacterSelectScene);
        });

        _joinGameBtn.onClick.AddListener(() =>
        {
            LobbyManager.Instance.StartClient();
            LevelLoader.LoadNetwork(LevelEnum.CharacterSelectScene);
        });
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
