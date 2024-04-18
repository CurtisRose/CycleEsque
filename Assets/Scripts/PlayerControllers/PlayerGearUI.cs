using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGearUI : MonoBehaviour
{
    [SerializeField] GearManager gearManager;

    [SerializeField] Image backpackBackgroundImage;
    [SerializeField] Image helmetBackgroundImage;
    [SerializeField] Image armorBackgroundImage;

    private void Awake()
    {
        gearManager.OnBackpackChanged += HandleBackpackChange;
        gearManager.OnHelmetChanged += HandleHelmetChange;
        gearManager.OnArmorChanged += HandleArmorChange;
    }

    private void HandleBackpackChange(SharedItemData itemData)
    {
        if (itemData == null)
        {
            backpackBackgroundImage.enabled = false;
        } else
        {
            backpackBackgroundImage.enabled = true;
            backpackBackgroundImage.color = RarityColorManager.Instance.GetColorByRarity(itemData.Rarity);
        }
    }

    private void HandleHelmetChange(SharedItemData itemData)
    {
        if (itemData == null)
        {
            helmetBackgroundImage.enabled = false;
        }
        else
        {
            helmetBackgroundImage.enabled = true;
            helmetBackgroundImage.color = RarityColorManager.Instance.GetColorByRarity(itemData.Rarity);
        }
    }

    private void HandleArmorChange(SharedItemData itemData)
    {
        if (itemData == null)
        {
            armorBackgroundImage.enabled = false;
        }
        else
        {
            armorBackgroundImage.enabled = true;
            armorBackgroundImage.color = RarityColorManager.Instance.GetColorByRarity(itemData.Rarity);
        }
    }
}
