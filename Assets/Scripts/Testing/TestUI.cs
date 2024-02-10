using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    [SerializeField]
    private Button _startHostBtn;

    [SerializeField]
    private Button _startClientBtn;

    private void Awake()
    {
        _startHostBtn.onClick.AddListener(() =>
        {
            Debug.Log("Started Host");
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        _startClientBtn.onClick.AddListener(() =>
        {
            Debug.Log("Started Client");
            NetworkManager.Singleton.StartClient();
            Hide();
        });

    }

    private void Hide()
    {
        gameObject.SetActive(false);
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
