using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGearManager : MonoBehaviour
{
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] PlayerWeaponSwitcher playerWeaponSwitcher;

    // GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 }
    [SerializeField] WorldItem[] gearItems;

    [SerializeField] List<Transform> gearStorageLocations;

    [SerializeField] Transform weaponPositionHands;
    [SerializeField] Transform weaponPositionHip;

    public delegate void PrimaryChanged(Gun gun);
    public event PrimaryChanged OnPrimaryChanged;

    public delegate void SecondaryChanged(Gun gun);
    public event SecondaryChanged OnSecondaryChanged;

    public delegate void HelmetChanged(SharedItemData itemData);
    public event HelmetChanged OnHelmetChanged;

    public delegate void ArmorChanged(SharedItemData itemData);
    public event ArmorChanged OnArmorChanged;

    public delegate void BackpackChanged(SharedItemData itemData);
    public event BackpackChanged OnBackpackChanged;

    // Sounds
    AudioSource audioSource;
    [SerializeField] List<AudioClip> gearRemovalSounds;
    SoundRandomizer gearRemovalRandomClips;

    private void Awake()
    {
        gearItems = new WorldItem[5];
        foreach (GearSlot gearSlot in playerInventory.GetGearSlots())
        {
            gearSlot.OnGearSlotsChanged += GearSlotChange;
        }
        playerWeaponSwitcher = GetComponent<PlayerWeaponSwitcher>();
        audioSource = GetComponent<AudioSource>();
        gearRemovalRandomClips = new SoundRandomizer(gearRemovalSounds);
    }

    private void GearSlotChange(GearSlot gearSlot)
    {
        if (gearSlot.GetItemType() == ItemType.PRIMARY_WEAPON)
        {
            if (gearSlot == playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1))
            {
                HandleGearSlotChange(GearSlotIdentifier.WEAPONSLOT1, gearSlot);
            }
            else
            {
                HandleGearSlotChange(GearSlotIdentifier.WEAPONSLOT2, gearSlot);
            }
        }
        else if (gearSlot.GetItemType() == ItemType.BACKPACK)
        {
            HandleGearSlotChange(GearSlotIdentifier.BACKPACK, gearSlot);
        }
        else if (gearSlot.GetItemType() == ItemType.HELMET)
        {
            HandleGearSlotChange(GearSlotIdentifier.HELMET, gearSlot);
        }
        else if (gearSlot.GetItemType() == ItemType.ARMOR)
        {
            HandleGearSlotChange(GearSlotIdentifier.ARMOR, gearSlot);
        }
        else if (gearSlot.GetItemType() == ItemType.OTHER)
        {
            Debug.Log("Error: What Kind of Gear is That?");
            //HandleGearSlotChange(GearSlotIdentifier.WEAPONSLOT1, gearSlot);
        }
    }

    private void HandleGearSlotChange(GearSlotIdentifier identifier, GearSlot gearSlot)
    {
        if (gearItems[(int)identifier] != null)
        {
            Destroy(gearItems[(int)identifier].gameObject);
            gearItems[(int)identifier] = null;
        }
        SharedItemData sharedItemData = null;
        if (gearSlot.HasItem())
        {
            gearItems[(int)identifier] = PlayerItemSpawner.Instance.SpawnItem(gearSlot.GetItemInSlot().itemInstance, gearStorageLocations[(int)identifier]);
            gearItems[(int)identifier].Equip();
            sharedItemData = gearItems[(int)identifier].GetBaseItem();
        }

        if (identifier == GearSlotIdentifier.WEAPONSLOT1)
        {
            if (OnPrimaryChanged != null) OnPrimaryChanged((Gun)gearItems[(int)identifier]);
        }
        else if (identifier == GearSlotIdentifier.WEAPONSLOT2)
        {
            if (OnPrimaryChanged != null) OnSecondaryChanged((Gun)gearItems[(int)identifier]);
        }
        else if (identifier == GearSlotIdentifier.BACKPACK)
        {
            MakeSound(gearRemovalRandomClips.GetRandomClip());
            if (OnBackpackChanged != null) OnBackpackChanged(sharedItemData);
        }
        else if (identifier == GearSlotIdentifier.HELMET)
        {
            MakeSound(gearRemovalRandomClips.GetRandomClip());
            if (OnHelmetChanged != null) OnHelmetChanged(sharedItemData);
        }
        else if (identifier == GearSlotIdentifier.ARMOR)
        {
            MakeSound(gearRemovalRandomClips.GetRandomClip());
            if (OnArmorChanged != null) OnArmorChanged(sharedItemData);
        }
    }

    public Gun GetGunInHands()
    {
        return playerWeaponSwitcher.GetGunInHands();
    }

    public Gun GunGetGunOnHip()
    {
        return playerWeaponSwitcher.GetGunOnHip();
    }

    public void MakeSound(AudioClip audioClip)
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f); // Adjust pitch slightly
        audioSource.volume = Random.Range(0.8f, 1.0f); // Adjust volume slightly
        audioSource.PlayOneShot(audioClip);
    }
}
