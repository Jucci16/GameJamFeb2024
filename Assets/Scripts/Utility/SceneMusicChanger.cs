using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SceneMusicChanger : MonoBehaviour
{
    [SerializeField] AudioResource _audioResource;

    private bool _isPlaying = false;

    // Update is called once per frame
    void Update()
    {
        if (_isPlaying) return;

        if (_audioResource is null || MusicPlayer.Instance is null) return;

        _isPlaying = true;
        MusicPlayer.Instance.ChangeSong(_audioResource);
    }
}
