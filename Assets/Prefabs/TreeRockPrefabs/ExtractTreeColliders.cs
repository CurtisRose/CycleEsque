using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class ExtractTreeColliders : MonoBehaviour
{
    [SerializeField]
    private Terrain terrain;

    private void Reset()
    {
        terrain = GetComponent<Terrain>();

        Extract();
    }

    [ContextMenu("Extract")]
    public void Extract()
    {
        Collider[] colliders = terrain.GetComponentsInChildren<Collider>();

        //Skip the first, since its the Terrain Collider
        for (int i = 1; i < colliders.Length; i++)
        {
            //Delete all previously created colliders first
            DestroyImmediate(colliders[i].gameObject);
        }

        for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
        {
            TreePrototype tree = terrain.terrainData.treePrototypes[i];

            //Get all instances matching the prefab index
            TreeInstance[] instances = terrain.terrainData.treeInstances.Where(x => x.prototypeIndex == i).ToArray();

            for (int j = 0; j < instances.Length; j++)
            {
                //Un-normalize positions so they're in world-space
                instances[j].position = Vector3.Scale(instances[j].position, terrain.terrainData.size);
                instances[j].position += terrain.GetPosition();

                //Fetch the collider from the prefab object parent
                CapsuleCollider prefabCollider = tree.prefab.GetComponent<CapsuleCollider>();
                if (!prefabCollider) continue;

                GameObject obj = new GameObject();
                obj.name = tree.prefab.name + j;

                CapsuleCollider objCollider = obj.AddComponent<CapsuleCollider>();

                objCollider.center = prefabCollider.center;
                objCollider.height = prefabCollider.height;
                objCollider.radius = prefabCollider.radius;

                if (terrain.preserveTreePrototypeLayers) obj.layer = tree.prefab.layer;
                else obj.layer = terrain.gameObject.layer;

                obj.transform.position = instances[j].position;
                obj.transform.parent = terrain.transform;
            }
        }
    }
}