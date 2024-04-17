using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{ 
    public static ProjectilePool Instance;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private bool expandable = true; // Option to control whether the pool can expand

    private Queue<Projectile> projectiles = new Queue<Projectile>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            Projectile proj = Instantiate(projectilePrefab);
            proj.transform.SetParent(this.transform);
            proj.gameObject.SetActive(false);
            projectiles.Enqueue(proj);
        }
    }

    public Projectile GetProjectile()
    {

        if (projectiles.Count > 0)
        {
            Projectile proj = projectiles.Dequeue();
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

    private Projectile AddProjectileToPool()
    {
        Projectile proj = Instantiate(projectilePrefab);
        proj.transform.SetParent(this.transform);
        proj.gameObject.SetActive(false); // Instantiate it inactive
        projectiles.Enqueue(proj);
        poolSize++;
        return proj;
    }

    public void ReturnProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        projectiles.Enqueue(projectile);
    }
}