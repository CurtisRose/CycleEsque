using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeployToSurface : MonoBehaviour
{
    [SerializeField] StashManager stashManager;

    public void DeployToSurfaceAction()
    {
        //PlayerData playerData = new PlayerData(PlayerInventory.Instance);
        //GameManager.Instance.SavePlayerData();
        stashManager.SaveStashToJson();
        SceneManagerHelper.LoadSceneWithPlayerData("MapDevelopment");
    }
}
