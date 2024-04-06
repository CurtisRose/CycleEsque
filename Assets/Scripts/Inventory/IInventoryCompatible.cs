using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryCompatible
{
    public Sprite GetSprite();
    public bool IsStackable();
    public int MaxStackSize();

    public bool IsEquatable(IInventoryCompatible other);
}
