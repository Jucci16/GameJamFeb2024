using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    public void ChangeSong(AudioResource audio)
    {
        var audioSource = GetComponent<AudioSource>();
        audioSource.resource = audio;
        audioSource.Play();
    }

    private void Awake()
    {
        if(Instance != null) {
            var audioSource = Instance.GetComponent<AudioSource>();
            audioSource.Stop();
        }
        Instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
