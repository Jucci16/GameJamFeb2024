using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLoader
{
    public static void Load(LevelEnum level)
    {
        SceneManager.LoadScene(level.ToString(), LoadSceneMode.Single);
    }

    public static void LoadNetwork(LevelEnum level)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(level.ToString(), LoadSceneMode.Single);
    }
}
