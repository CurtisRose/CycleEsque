using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    public SharedItemData sharedData;
    public Dictionary<string, object> uniqueData = new Dictionary<string, object>();

    public ItemInstance(SharedItemData sharedData)
    {
        this.sharedData = sharedData;
    }

    public bool SetProperty(string key, object value)
    {
        if (sharedData.allowedKeys.Contains(key))
        {
            uniqueData[key] = value;
            return true;
        }
        Debug.LogWarning($"Property key '{key}' is not valid for item type '{sharedData.ItemType.ToString()}'");
        return false;
    }

    public object GetProperty(string key)
    {
        if (uniqueData.TryGetValue(key, out var value))
        {
            return value;
        }
        return null; // Optionally, throw an exception or handle this case
    }
}
