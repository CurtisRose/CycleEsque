using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManagerHelper : MonoBehaviour
{
    public static void LoadSceneWithPlayerData(string sceneName)
    {
        // Save the player data to json
        GameManager.Instance.SavePlayerData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}