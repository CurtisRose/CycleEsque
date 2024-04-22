using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeployToSurface : MonoBehaviour
{
    [SerializeField] StashManager stashManager;

    public void DeployToSurfaceAction()
    {
        stashManager.SaveStashToJson();
        SceneManager.LoadScene("MapDevelopment");
    }
}
