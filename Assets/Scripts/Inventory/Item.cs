using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; }

    [field: SerializeField] public int maxStackSize { get; private set; }
    
    [field: SerializeField] public Sprite image { get; private set; }
    [field: SerializeField] public bool stackable { get; private set; }
}
