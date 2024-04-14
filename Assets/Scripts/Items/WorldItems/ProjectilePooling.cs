using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{ 
    public static ProjectilePool Instance;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private bool expandable = true; // Option to control whether the pool can expand

    private Queue<GameObject> projectiles = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject proj = Instantiate(projectilePrefab);
            proj.transform.SetParent(this.transform);
            proj.SetActive(false);
            projectiles.Enqueue(proj);
        }
    }

    public GameObject GetProjectile()
    {

        if (projectiles.Count > 0)
        {
            GameObject proj = projectiles.Dequeue();
            proj.SetActive(false); // Ensure projectile is inactive when returned
            return proj;
        }
        else if (expandable)
        {
            // Dynamically expand the pool by adding a new projectile
            return AddProjectileToPool();
        }

        // Return null or handle this situation as appropriate if the pool can't expand
        Debug.LogWarning("No projectiles available in pool and expansion is disabled!");
        return null;
    }

    private GameObject AddProjectileToPool()
    {
        GameObject proj = Instantiate(projectilePrefab);
        proj.transform.SetParent(this.transform);
        proj.SetActive(false); // Instantiate it inactive
        projectiles.Enqueue(proj);
        poolSize++;
        return proj;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        projectiles.Enqueue(projectile);
    }
}