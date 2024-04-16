/*using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AddTreesToTerrain : EditorWindow
{
    [MenuItem("Tools/Add Trees To Terrain")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddTreesToTerrain));
    }

    void OnGUI()
    {
        if (GUILayout.Button("Add Trees to Terrain"))
        {
            AddTrees();
        }
    }

    static void AddTrees()
    {
        // Path to the folder where tree prefabs are stored
        string folderPath = "Assets/Prefabs/TreeRockPrefabs";
        var guids = AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });
        List<TreePrototype> newTrees = new List<TreePrototype>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AddTreeFromPath(path, newTrees);
        }

        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain != null)
        {
            terrain.terrainData.treePrototypes = newTrees.ToArray();
            terrain.terrainData.RefreshPrototypes();
            EditorUtility.SetDirty(terrain);
        }
    }

    static void AddTreeFromPath(string path, List<TreePrototype> treeList)
    {
        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (treePrefab != null)
        {
            TreePrototype treeProto = new TreePrototype();
            treeProto.prefab = treePrefab;
            treeList.Add(treeProto);
        }
    }
}
*/