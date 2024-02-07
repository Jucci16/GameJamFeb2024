using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestCharacterSelectUI : MonoBehaviour
{
    [SerializeField]
    private Button _readyButton;

    private void Awake()
    {
        _readyButton.onClick.AddListener(() => { CharacterSelectReady.Instance.SetPlayerReady(); });
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
