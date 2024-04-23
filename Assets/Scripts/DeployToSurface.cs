using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeployToSurface : MonoBehaviour
{
    [SerializeField] StashInventoryManager stashManager;

    public void DeployToSurfaceAction()
    {
        //PlayerData playerData = new PlayerData(PlayerInventory.Instance);
        //GameManager.Instance.SavePlayerData();
        stashManager.SaveStash();
        SceneManagerHelper.LoadSceneWithPlayerData("MapDevelopment");
    }
}
