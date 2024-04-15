using System;


public enum ItemAttributeKey
{
    NumItemsInStack,
    AmmoCount, 
    ArmorRemaining
}

public static class ItemAttributeKeys
{
    public static string KeyToString(ItemAttributeKey key)
    {
        return Enum.GetName(typeof(ItemAttributeKey), key);
    }
}