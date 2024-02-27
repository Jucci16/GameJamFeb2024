using System;
using UnityEngine;
using TMPro;

public class RespawnCountdown : MonoBehaviour
{
    public static RespawnCountdown Instance;

    public const float respawnTimeSeconds = 3.0f;
    private float? _timeElapsed = null;

    [SerializeField]
    private TextMeshProUGUI _countText;

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

    public void Show()
    {
        gameObject.SetActive(true);
    }

    void Update()
    {
        if(_timeElapsed != null) {
            _timeElapsed += Time.deltaTime;
            if(_timeElapsed >= respawnTimeSeconds) {
                Hide();
                _timeElapsed = null;
            } else {
                _countText.text = ((int)Math.Ceiling(Convert.ToDecimal(respawnTimeSeconds - _timeElapsed!))).ToString();
            }
        }
    }

    public void StartRespawnCountdown() {
        if(GameOverUI.Instance.isGameOver()) return;

        Show();
        _timeElapsed = 0.0f;
        _countText.text = ((int)respawnTimeSeconds).ToString();
    }
}
