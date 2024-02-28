using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchUIManager : MonoBehaviour
{
    private const string _readytoFireLabel = "Ready to Fire";
    private const string _reloadingLabel = "Reloading";
    private const float _reloadTimeSeconds = 3.0f;
    private const int _startingLives = 3;

    public static MatchUIManager instance;
    public bool isReloading;


    private AudioSource _reloadingAudioSource;

    [SerializeField]
    private TextMeshProUGUI _reloadText;

    [SerializeField]
    private Image _livesRemainingIcon;

    private List<Image> _livesRemainingIcons;

    private void Awake() {
        instance = this;
    }

    void Start()
    {
        _reloadingAudioSource = GetComponent<AudioSource>();
        SpawnLivesRemainingIcons();
        ResetReload();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void SpawnLivesRemainingIcons() {
        _livesRemainingIcons = new List<Image>{_livesRemainingIcon};
        var iconRectTransform = _livesRemainingIcon.GetComponent<RectTransform>();
        var lastIconPos = iconRectTransform.anchoredPosition;
        for(int i = 0; i < _startingLives - 1; i++) {
            var newIcon = Instantiate(_livesRemainingIcon, this.transform);
            var newIconPos = lastIconPos - new Vector2(iconRectTransform.rect.width + 5f, 0f);
            newIcon.GetComponent<RectTransform>().anchoredPosition = newIconPos;
            lastIconPos = newIconPos;
            _livesRemainingIcons.Add(newIcon);
        }
    }

    public void SetPlayerColor(Color color) {
        foreach(var icon in _livesRemainingIcons) {
            icon.color = color;
        }
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

    // Resturns true if no lives remaining.
    public bool DecrementLifeCount() {
        var endIcon = _livesRemainingIcons[_livesRemainingIcons.Count - 1];
        _livesRemainingIcons.Remove(endIcon);
        Destroy(endIcon);
        if(_livesRemainingIcons.Count == 0) {
            GameOverUI.Instance.Show(GameOverStatus.loser);
            return true;
        }
        return false;
    }
}
