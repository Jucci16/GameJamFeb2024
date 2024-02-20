using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchUIManager : MonoBehaviour
{
    private const string _readytoFireLabel = "Ready to Fire";
    private const string _reloadingLabel = "Reloading";
    private const float _reloadTimeSeconds = 3.0f;

    public static MatchUIManager instance;
    public bool isReloading;


    private AudioSource _reloadingAudioSource;

    [SerializeField]
    private TextMeshProUGUI _reloadText;

    private void Awake() {
        instance = this;
    }

    void Start()
    {
        _reloadingAudioSource = GetComponent<AudioSource>();
        ResetReload();
    }

    public void StartReload() {
        _reloadText.color = Color.red;
        _reloadText.text = _reloadingLabel;
        isReloading = true;
        _reloadingAudioSource.Play(0);
        // Execute the ResetReload function after the reload time has elapsed. 
        Invoke("ResetReload", _reloadTimeSeconds);
    }

    private void ResetReload() {
        _reloadText.color = Color.green;
        _reloadText.text = _readytoFireLabel;
        isReloading = false;
    }
}
