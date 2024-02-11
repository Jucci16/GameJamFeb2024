using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectColorSingleUI : MonoBehaviour
{
    [SerializeField] private int _colorId;
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _selectedImageGameObjected;

    private void Start()
    {
        LobbyManager.Instance.OnPlayerDataListChanged += LobbyManager_OnPlayerDataListChanged;
        _image.color = LobbyManager.Instance.GetPlayerColor(_colorId);
        UpdateIsSelected();
    }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            LobbyManager.Instance.ChangePlayerColor(_colorId);
        });
    }

    private void UpdateIsSelected()
    {
        _selectedImageGameObjected.SetActive(LobbyManager.Instance.GetPlayerData().ColorId == _colorId);
    }

    private void LobbyManager_OnPlayerDataListChanged(object sender, EventArgs e)
    {
        UpdateIsSelected();
    }
}
