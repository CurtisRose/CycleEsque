using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    public SharedItemData sharedData;
    protected Dictionary<string, object> uniqueData = new Dictionary<string, object>();

    public ItemInstance(SharedItemData sharedData)
    {
        this.sharedData = sharedData;
        foreach (string key in sharedData.allowedKeys)
        {
            uniqueData.Add(key, null);
        }
    }

    public bool SetProperty(ItemAttributeKey key, object value)
    {
        string keyString = ItemAttributeKeys.KeyToString(key);

        if (sharedData.allowedKeys.Contains(keyString))
        {
            uniqueData[keyString] = value;
            return true;
        }
        Debug.LogWarning($"Property key '{keyString}' is not valid for item type '{sharedData.ItemType.ToString()}'");
        return false;
    }

    public object GetProperty(ItemAttributeKey key)
    {
        string keyString = ItemAttributeKeys.KeyToString(key);
        if (sharedData.allowedKeys.Contains(keyString)) 
        {
            if (uniqueData.TryGetValue(keyString, out object value))
            {
                return value;
            }
        }
        Debug.LogWarning($"Property key '{keyString}' is not valid for item type '{sharedData.ItemType.ToString()}'");
        return null; // Optionally, throw an exception or handle this case
    }

    public ItemInstance Clone()
    {
        // Create a new ItemInstance using the same SharedItemData
        ItemInstance clone = new ItemInstance(this.sharedData);

        // Copy each key-value pair in uniqueData
        foreach (var kvp in this.uniqueData)
        {
            // This assumes that the values are simple types or are immutable;
            // if this is not the case, a deeper copy might be necessary.
            clone.uniqueData[kvp.Key] = kvp.Value;
        }

        return clone;
    }
}
