using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 };

[System.Serializable]
public struct InventoryStartItem
{
    public SharedItemData itemData;
    public int quantity;
}

public class PlayerInventory : Inventory {
	public static PlayerInventory Instance;

	[SerializeField] protected float inventoryWeightLimit;
    [SerializeField] protected float currentWeight;

	[SerializeField] protected InventoryStartItem[] startItems;
	public GameObject backpackInventory;
	[SerializeField] protected List<GearSlot> gearSlots;
	[SerializeField] protected TMP_Text weightText; // "BACKPACK 0.0/0.0"

	public delegate void InventoryChanged();
	public event InventoryChanged OnInventoryChanged;

	public delegate void ItemDropped(ItemInstance itemInstance);
	public event ItemDropped OnItemDropped;

	private void Awake() {
		if (Instance != null) {
			Destroy(this);
		} else {
			Instance = this;
		}
	}

	protected void Start() {
		if (PlayerInventoryMenu.Instance != null) {
			PlayerInventoryMenu.Instance.Open();
		}

		foreach (InventoryStartItem startItem in startItems) {
			//ItemInstance itemInstance = new ItemInstance(startItem);
			WorldItem testItem = PlayerItemSpawner.Instance.GetPrefab(startItem.itemData);
			ItemInstance testInstance = testItem.CreateNewItemInstance(startItem.itemData);
			if (startItem.itemData.Stackable) {
				testInstance.SetProperty(ItemAttributeKey.NumItemsInStack, startItem.quantity);
			}

			//itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, 1);
			AddItem(testInstance);
		}

		if (PlayerInventoryMenu.Instance != null) {
			PlayerInventoryMenu.Instance.Close();
		}
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {

			if (PlayerInventoryMenu.Instance != null) {
				if (!PlayerInventoryMenu.Instance.IsOpen()) {
					MenuManager.Instance.OpenMenu(PlayerInventoryMenu.Instance);
				} else {
					MenuManager.Instance.CloseMenu(PlayerInventoryMenu.Instance);
				}
			}
		}
		/*if (Input.GetKey(KeyCode.E))
        {
            ItemInstance ammo = new ItemInstance(startItems[5].itemData);
            ammo.SetProperty(ItemAttributeKey.NumItemsInStack, 1);
            AddItem(ammo);
        }*/
	}

    public float GetCurrentInventoryWeight()
    {
        return currentWeight;
    }

    public override bool AddItem(ItemInstance itemInstance)
    {
        int numItems = HowManyItemsCanBeAdded(itemInstance);

        if (numItems <= 0)
        {
            Debug.Log("No Room In Inventory");
            return false;
        }

        bool updated = false; // Flag to track if the inventory was updated

        // If the item is stackable, try to add it to existing slots with the same item
        if (itemInstance.sharedData.Stackable)
        {
            int startItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
            int remainingItems = FillExistingStacks(itemInstance);
            int itemsAdded = startItems - remainingItems;
            UpdateWeight(itemInstance.sharedData.Weight * itemsAdded);
            if (remainingItems < numItems)
            {
                updated = true;  // Update occurred if we added some items to existing stacks
            }
            numItems = remainingItems;
        }

        // If there are items left after trying to stack them in existing slots, or if the item is not stackable
        if (numItems > 0)
        {
            int itemsLeftToAdd = FillEmptySlots(itemInstance);
            if (itemsLeftToAdd < numItems)
            {
                updated = true;  // Update occurred if we added some items to new slots
            }
        }

		if (updated) {
			OnInventoryChanged?.Invoke();
		}

		return updated;
    }

    public bool AddItem(WorldItem item)
    {
        int numItems = item.GetNumberOfItems();
        bool partialOnly = false;
        if (item.GetWeight() > GetInventoryWeightLimit() - currentWeight)
        {
            if (item.GetBaseItem().Stackable)
            {
                float weightToFillBackpack = GetInventoryWeightLimit() - currentWeight;
                numItems = (int)Mathf.Floor(weightToFillBackpack / item.GetBaseItem().Weight);
                partialOnly = true;
                if (numItems == 0)
                {
                    return false;
                }
            } else
            {
                return false;
            }
        }

        ItemInstance itemPickedUp = item.CreateItemInstance();
        itemPickedUp.SetProperty(ItemAttributeKey.NumItemsInStack, numItems);
        bool successCheck = AddItem(itemPickedUp);

        if (partialOnly)
        {
            item.ChangeNumberOfItems(-numItems);
            return false;
        }

        return successCheck && !partialOnly;
    }

    public override bool AddItem(InventorySlot slotToAddTo, InventoryItem itemToAdd)
    {
        // If either slot is a gear slot, make sure both are allowed to fit the item that's being swapped
        if (slotToAddTo as GearSlot) {
            if (((GearSlot) slotToAddTo).GetItemType() != itemToAdd.GetItemType()) {
                return false;
            }
        }

        // If itemslot has item, swap, unless stackable and same item type
        if (slotToAddTo.HasItem())
        {
			InventoryItem itemInSlot = slotToAddTo.GetItemInSlot();
			InventorySlot otherSlot = itemToAdd.GetCurrentInventorySlot();

			// Check that, if the other slot is a gear slot, then the item in slot is the correct type
            if (otherSlot as GearSlot) {
                if (((GearSlot) otherSlot).GetItemType() != itemInSlot.GetItemType()) {
					return false;
				}
            }


            if (itemToAdd.itemInstance.sharedData.Stackable &&
                itemToAdd.GetItemType() == itemInSlot.GetItemType())
            {
                // TODO: Need to do something like fill it with what it can
                //return false;
            }

            InventoryItem itemAlreadyHere = itemInSlot;
            float weightAfterSwap = currentWeight;
            float weightLimitAfterSwap = GetInventoryWeightLimit();
            bool potentiallySwappable = true;

            // Check to see if it's too heavy for inventory
            if (slotToAddTo.ContributesToWeight() ||
				itemToAdd.itemInstance.sharedData.ItemType == ItemType.BACKPACK && slotToAddTo as GearSlot)
            {
                // If the other slot is the backpack slot then recalculate the inventory size
                if (slotToAddTo as GearSlot && itemInSlot.GetItemType() == ItemType.BACKPACK)
                {
                    weightLimitAfterSwap += 
                        ((BackpackItem)itemToAdd.itemInstance.sharedData).CarryCapacity -
						((BackpackItem)itemAlreadyHere.itemInstance.sharedData).CarryCapacity;
				}
				if (slotToAddTo.ContributesToWeight()) {
					weightAfterSwap = weightAfterSwap + itemToAdd.GetTotalWeight() - itemAlreadyHere.GetTotalWeight();
				}
				if (weightAfterSwap > weightLimitAfterSwap) {
					potentiallySwappable = false;
				}
			}

            if (potentiallySwappable) {
				// Do the same thing for the other slot and other inventory

				if (otherSlot.GetInventory() as PlayerInventory) {
					weightAfterSwap = ((PlayerInventory)otherSlot.GetInventory()).currentWeight;
					weightLimitAfterSwap = ((PlayerInventory)otherSlot.GetInventory()).GetInventoryWeightLimit();
				}

                if (otherSlot.ContributesToWeight() ||
                    itemToAdd.itemInstance.sharedData.ItemType == ItemType.BACKPACK && otherSlot as GearSlot) {
                    // If the this slot is the backpack slot then recalculate the inventory size
                    if (otherSlot as GearSlot && itemToAdd.itemInstance.sharedData.ItemType == ItemType.BACKPACK) {
                        weightLimitAfterSwap +=
                            ((BackpackItem)itemAlreadyHere.itemInstance.sharedData).CarryCapacity -
                            ((BackpackItem)itemToAdd.itemInstance.sharedData).CarryCapacity;
                    }
                    if (otherSlot.ContributesToWeight()) {
                        weightAfterSwap = weightAfterSwap - itemAlreadyHere.GetTotalWeight() + itemToAdd.GetTotalWeight();
                    }
                    if (weightAfterSwap > weightLimitAfterSwap) {
                        potentiallySwappable = false;
                    }
                }
            }

            if (potentiallySwappable) {
				Swap(slotToAddTo, itemToAdd);
			} else {
                return false;
            }
        }
        else
        {
            InventorySlot otherSlot = itemToAdd.GetCurrentInventorySlot();
            Inventory otherInventory = otherSlot.GetInventory();
            
            // Calculate new carry capacities after potential swap
            float inventoryCarryCapacityAfterSwap = GetInventoryWeightLimit();
            float otherInventoryCarryCapacityAfterSwap = 99999;

			if (otherInventory as PlayerInventory) {
				otherInventoryCarryCapacityAfterSwap = ((PlayerInventory)otherInventory).GetInventoryWeightLimit();
			}
            if (itemToAdd.itemInstance.sharedData.ItemType == ItemType.BACKPACK) {
                if (otherSlot as GearSlot || slotToAddTo as GearSlot) {
                    inventoryCarryCapacityAfterSwap += ((BackpackItem)itemToAdd.itemInstance.sharedData).CarryCapacity;
                    otherInventoryCarryCapacityAfterSwap -= ((BackpackItem)itemToAdd.itemInstance.sharedData).CarryCapacity;
                }
            }

			bool potentiallySwappable = true;
			// This is if the inventories are differnt inventories.
			if (otherInventory != this) {
                // Calculate new weights after potential swap
                float weightAfterSwap = currentWeight;
                float otherWeightAfterSwap = 0;
				if (otherInventory as PlayerInventory) {
					otherWeightAfterSwap = ((PlayerInventory)otherInventory).currentWeight;
				}

				if (slotToAddTo.ContributesToWeight()) {
                    weightAfterSwap += itemToAdd.GetTotalWeight();
                }
                if (otherSlot.ContributesToWeight()) {
                    otherWeightAfterSwap -= itemToAdd.GetTotalWeight();
                }

                // Check to see if it's too heavy for inventory
                if (weightAfterSwap > inventoryCarryCapacityAfterSwap) {
                    potentiallySwappable = false;
                }
                if (otherWeightAfterSwap > otherInventoryCarryCapacityAfterSwap) {
                    potentiallySwappable = false;
                }
            } else { // If the inventories are the same
				// If adding to a gear slot... it doesn't matter, doesn't matter which gear slot, it's empty, so weight goes down, and carry capacity goes up if backpack
                if ((slotToAddTo as GearSlot)) {
					// No check?
				}
                // If moving from a gear slot, then it does matter, carry capacity goes down, current weight goes up
                else if(otherSlot as GearSlot) {
					float weightAfterSwap = currentWeight + itemToAdd.GetTotalWeight();
                    if (weightAfterSwap > otherInventoryCarryCapacityAfterSwap) {
						potentiallySwappable = false;
					}
				} 
                // If neither are a gear slot, then it's just a normal swap inside the inventory, no weight change so it's good?
                else {
                    // No Check?
                }
            }

			if (potentiallySwappable) {
				PlaceItem(itemToAdd, slotToAddTo);
			} else {
				return false;
			}
		}

        return true;
    }

    public override int HowManyItemsCanBeAdded(ItemInstance itemInstance)
    {
        int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);

        // Calculate the available weight capacity
        float availableWeight = GetInventoryWeightLimit() - currentWeight;

        // Calculate the maximum number of items that can be added based on weight
        int maxItemsByWeight = (int)(availableWeight / itemInstance.sharedData.Weight);

        // Determine the actual number of items that can be added
        int itemsToAdd = Mathf.Min(numItems, maxItemsByWeight);

        return itemsToAdd;
    }

    protected override int FillExistingStacks(ItemInstance itemInstance)
    {
        // TODO probably need to do something about weight here
        return base.FillExistingStacks(itemInstance);
    }

    public override int FillEmptySlots(ItemInstance itemInstance)
    {
        int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);

        // Calculate the available weight capacity
        float availableWeight = GetInventoryWeightLimit() - currentWeight;

        // Calculate the maximum number of items that can be added based on weight
        int maxItemsByWeight = (int)(availableWeight / itemInstance.sharedData.Weight);

        // Determine the actual number of items that can be added
        int itemsToAdd = Mathf.Min(numItems, maxItemsByWeight);

        if (itemsToAdd <= 0)
        {
            Debug.Log("Not enough weight capacity to add these items");
            return numItems; // Not enough weight capacity to add any of the items, return the original number
        }

        foreach (InventorySlot slot in inventorySlots)
        {
            if (!slot.HasItem() && itemsToAdd > 0)
            {
                int itemsThatCanBeAdded = itemInstance.sharedData.Stackable ? Mathf.Min(itemInstance.sharedData.MaxStackSize, itemsToAdd) : 1;
                ItemInstance newItemInstance = itemInstance.Clone(); // Create a deep copy of itemInstance
                newItemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, itemsThatCanBeAdded);

                // Create item adds the weight at some point.
                CreateNewItem(newItemInstance, slot);  // Create a new item in the slot
                //UpdateWeight(newItemInstance.sharedData.Weight * itemsThatCanBeAdded);  // Update the total weight

                itemsToAdd -= itemsThatCanBeAdded; // Reduce the number of items left to add
                if (itemsToAdd == 0) return 0; // Successfully added all items
            }
        }

        if (itemsToAdd > 0)
        {
            Debug.Log("Inventory is full or not enough slots to accommodate all items.");
        }

        return itemsToAdd; // Return the number of items left that could not be added
    }

    public int GetNumberOfItemsOfType(ItemType type)
    {
        int numItems = 0;
        foreach(InventorySlot inventorySlot in inventorySlots)
        {
            InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
            if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type)
            {
                if (inventorySlot.GetItemInSlot().itemInstance.sharedData.Stackable)
                {
                    numItems += inventorySlot.GetItemInSlot().GetItemCount();
                } else
                {
                    numItems++;
                }
            }
        }
        return numItems;
    }

    // TODO:
    public bool RemoveItem(ItemInstance itemInstance)
    {
        Debug.Log("This Function has NOT Been Implemented");
        return true;
    }

    public bool RemoveItemOfType(ItemType type, int numItems = 1)
    {
        foreach (InventorySlot inventorySlot in inventorySlots)
        {
            InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
            if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type)
            {
                if (itemInSlot.GetItemCount() > numItems)
                {
                    itemInSlot.AddToItemCount(-numItems);
                    UpdateWeight(-(numItems * itemInSlot.itemInstance.sharedData.Weight));
                    return true;
                } else if (itemInSlot.GetItemCount() == numItems)
                {
                    inventorySlot.RemoveItemFromSlot();
                    Destroy(itemInSlot.gameObject);
                    return true;
                } else
                {
                    inventorySlot.RemoveItemFromSlot();
                    Destroy(itemInSlot.gameObject);
                    numItems -= itemInSlot.GetItemCount();
                }
            }
        }
        return false;
    }

    protected override void CreateNewItem(ItemInstance itemInstance, InventorySlot inventorySlot)
    {
        base.CreateNewItem(itemInstance, inventorySlot);
		// Base.CreateNewItem puts the item in sthe inventory slot, but we need to update the weight here
        InventoryItem inventoryItem = inventorySlot.GetItemInSlot();
		if (inventorySlot.ContributesToWeight())
        {
            UpdateWeight(inventoryItem.GetTotalWeight());
        }
    }

    public override void Swap(InventorySlot inventorySlot, InventoryItem incomingItem)
    {
        if (inventorySlot.HasItem())
        {
            InventoryItem inventoryItemAlreadyHere = inventorySlot.GetItemInSlot();
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot();
            inventorySlot.RemoveItemFromSlot();

            otherSlot.SetItemInSlotAfterDrag(inventoryItemAlreadyHere);
            inventorySlot.SetItemInSlotAfterDrag(incomingItem);

            if (otherSlot.ContributesToWeight())
            {
                if (otherSlot.GetInventory() as PlayerInventory) {
					((PlayerInventory)otherSlot.GetInventory()).UpdateWeight(inventoryItemAlreadyHere.GetTotalWeight());
					((PlayerInventory)otherSlot.GetInventory()).UpdateWeight(-incomingItem.GetTotalWeight());
				}
            }
            if (inventorySlot.ContributesToWeight())
            {
                UpdateWeight(incomingItem.GetTotalWeight());
                UpdateWeight(-inventoryItemAlreadyHere.GetTotalWeight());
            }

            inventoryItemAlreadyHere.DoThingsAfterMove();
            incomingItem.DoThingsAfterMove();
        }
        else
        {
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot();
            if (otherSlot.ContributesToWeight())
            {
				if (otherSlot.GetInventory() as PlayerInventory) {
					((PlayerInventory)otherSlot.GetInventory()).UpdateWeight(-incomingItem.GetTotalWeight());
            
                }
			}   
            inventorySlot.SetItemInSlotAfterDrag(incomingItem);
            if (inventorySlot.ContributesToWeight())
            {
                if (inventorySlot.GetInventory() as PlayerInventory) {
					((PlayerInventory)inventorySlot.GetInventory()).UpdateWeight(incomingItem.GetTotalWeight());
				}
            }
            incomingItem.DoThingsAfterMove();
        }
    }

	public override void QuickEquip(InventorySlot inventorySlot) {
		InventoryItem itemToEquip = inventorySlot.GetItemInSlot();

		// First, anything that is other, can't be equipped.
		// Later, there may be other "Types" that can't be equipped, but for now this works.
		// Find the earliest slot to quick sort it in your inventory
		if (itemToEquip.GetItemType() >= ItemType.AMMO) {
			AddItemToEarliestEmptySlot(inventorySlot.GetItemInSlot());
			return;
		}

		// If the slot clicked is an inventory slot, equip item to gearSlot
		if (inventorySlots.Contains(inventorySlot)) {
			// Switch it with a gear slot\
			GearSlot gearSlotMatch = null;

			// If it's a weapon, prefer an empty slot, else, the first slot
			if (itemToEquip.GetItemType() == ItemType.WEAPON) {
				// Pick first slot if it's empty
				if (!gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1].HasItem()) {
					gearSlotMatch = gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1];
				}
				// Pick second slot if it's empty
				else if (!gearSlots[(int)GearSlotIdentifier.WEAPONSLOT2].HasItem()) {
					gearSlotMatch = gearSlots[(int)GearSlotIdentifier.WEAPONSLOT2];
				} else // Else, default to first slot
				{
					gearSlotMatch = gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1];
				}
			} else {
				foreach (GearSlot gearSlot in gearSlots) {
					if (gearSlot.GetItemType() == itemToEquip.GetItemType()) {
						gearSlotMatch = gearSlot;
						break;
					}
				}
			}

			if (gearSlotMatch == null) {
				Debug.LogError("Error: No gear slot matches this type. Gear slots are probably misconfigured.");
			}

			// Do Weight Check Before Swapping, this is swapping inventory item into gear
			bool weightCheck = false;
			{
				// Get the weight difference
				float weightAfterSwitch = currentWeight - itemToEquip.GetTotalWeight();
				if (gearSlotMatch.GetItemInSlot() != null) {
					weightAfterSwitch += gearSlotMatch.GetItemInSlot().GetTotalWeight();
				}
				float newCarryCapacity = GetInventoryWeightLimit();
				// Then check if it was a backpack switch to check the new carry weight
				if (itemToEquip.GetItemType() == ItemType.BACKPACK) {
					newCarryCapacity = GetInventoryWeightLimit() + ((BackpackItem)itemToEquip.itemInstance.sharedData).CarryCapacity;
				}

				if (newCarryCapacity >= weightAfterSwitch) {
					weightCheck = true;
				}
			}

			if (weightCheck) {
				// First move the item to an earlier slot.
				AddItemToEarliestEmptySlot(itemToEquip);
				// Then swap them, that way the gear ends up in the earlier slot
				Swap(gearSlotMatch, itemToEquip);
			}
		} else // If the slot was a gear slot then swap into inventory
		{
			// Add it to the inventory, find empty slot
			InventorySlot inventorySlotMatch = null;
			foreach (InventorySlot tempInventorySlot in inventorySlots) {
				if (!tempInventorySlot.HasItem()) {
					inventorySlotMatch = tempInventorySlot;
					break;
				}
			}

			if (inventorySlot = null) {
				Debug.Log("ERROR: No empty slots");
				// TODO: Add more slots dynamically
				// Make inventory a scrollable window
			}

			// Do Weight Check Before Swapping, this is swapping gear into the inventory
			bool weightCheck = false;
			{
				// Get the weight difference
				float weightAfterSwitch = currentWeight + itemToEquip.GetTotalWeight();

				float newCarryCapacity = GetInventoryWeightLimit();
				// Then check if it was a backpack switch to check the new carry weight
				if (itemToEquip.GetItemType() == ItemType.BACKPACK) {
					newCarryCapacity = GetInventoryWeightLimit();
				}

				if (newCarryCapacity >= weightAfterSwitch) {
					weightCheck = true;
				}
			}

			if (weightCheck) {
				Swap(inventorySlotMatch, itemToEquip);
			}
		}
		UpdateWeightText();
	}


    public override void SplitInventoryItem(InventoryItem inventoryItem)
    {
        if (inventoryItem.itemInstance.sharedData.Stackable)
        {
            if (inventoryItem.GetItemCount() <= 1)
            {
                return;
            }
            InventorySlot otherSlot = FindEarliestEmptySlot();
            // If there isn't another empty slot, then out of luck.
            if (otherSlot == null)
            {
                return;
            }
            // There should be no weight considerations since it's already in the bag
            int numItems = inventoryItem.GetItemCount();
            int newStackNum = Mathf.FloorToInt(numItems / 2);

            // Remove the correct number of items from the existing property, update the weight in the inventory accordingly, then update the stats.
            inventoryItem.itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItems - newStackNum);
            UpdateWeight(inventoryItem.itemInstance.sharedData.Weight * -newStackNum);

            // Create new itemInstance, set it's number, fill empty slot with it.
            ItemInstance newItem = new ItemInstance(inventoryItem.itemInstance.sharedData);
            newItem.SetProperty(ItemAttributeKey.NumItemsInStack, newStackNum);
            FillEmptySlots(newItem);
        }
    }

    public static void MoveAsManyAsYouCan(PlayerInventory inventory, InventorySlot inventorySlot, InventoryItem inventoryItem)
    {
        if (!inventoryItem.itemInstance.sharedData.Stackable) return;

        InventorySlot currentSlot = inventoryItem.GetCurrentInventorySlot();

        int numItemsInStack = inventoryItem.GetItemCount();

        // Calculate the available weight capacity
        float availableWeight = inventory.GetInventoryWeightLimit() - inventory.currentWeight;

        // Calculate the maximum number of items that can be added based on weight
        int maxItemsByWeight = (int)(availableWeight / inventoryItem.itemInstance.sharedData.Weight);

        if (maxItemsByWeight <= 0) return;

        // Remove the correct number of items from the existing property, update the weight in the inventory accordingly, then update the stats.
        inventoryItem.itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItemsInStack - maxItemsByWeight);
        if (currentSlot.partOfPlayerInventory && currentSlot.ContributesToWeight())
        {
            inventory.UpdateWeight(inventoryItem.itemInstance.sharedData.Weight * -maxItemsByWeight);
        }

        // Create new itemInstance, set it's number, fill empty slot with it.
        ItemInstance newItem = new ItemInstance(inventoryItem.itemInstance.sharedData);
        if ((!inventorySlot.partOfPlayerInventory && inventorySlot.ContributesToWeight()))
        {
            // Probably need to do something about weight in this instance, we'll see
        }
        newItem.SetProperty(ItemAttributeKey.NumItemsInStack, maxItemsByWeight);
        inventory.FillEmptySlots(newItem);
    }

	public float GetInventoryWeightLimit() {
		if (gearSlots[(int)GearSlotIdentifier.BACKPACK].GetItemInSlot() != null) {
			BackpackItem backpack = (BackpackItem)gearSlots[(int)GearSlotIdentifier.BACKPACK].GetItemInSlot().itemInstance.sharedData;

			return inventoryWeightLimit + backpack.CarryCapacity;
		}
		return inventoryWeightLimit;
	}

	public void UpdateWeight(float amount) {
        currentWeight += amount;
		UpdateWeightText();
	}

	public void UpdateWeightText() {
		weightText.text = "BACKPACK " + currentWeight.ToString() + "/" + GetInventoryWeightLimit();
	}

	public void StartShowSlotAcceptability(InventoryItem inventoryItem) {
		foreach (GearSlot gearSlot in gearSlots) {
			gearSlot.DisplayItemIndication(inventoryItem.GetItemType());
		}
	}

	public void EndShowSlotAcceptability(InventoryItem inventoryItem) {
		foreach (GearSlot gearSlot in gearSlots) {
			gearSlot.ResetItemIndication();
		}
	}

	public void StartInventoryItemMoved(InventoryItem inventoryItem) {
		StartShowSlotAcceptability(inventoryItem);
	}

	public void EndInventoryItemMoved(InventoryItem inventoryItem) {
		EndShowSlotAcceptability(inventoryItem);
		UpdateWeightText();
	}

	public bool EquipItemInstance(ItemInstance itemInstance, GearSlot gearSlot) {
		ItemType itemType = itemInstance.sharedData.ItemType;
		if (gearSlot.HasItem()) {
			return false;
		} else {
			CreateNewItem(itemInstance, gearSlot);
			return true;
		}

	}

	public List<GearSlot> GetGearSlots() {
		return gearSlots;
	}

	public GearSlot GetGearSlot(GearSlotIdentifier identifier) {
		return gearSlots[(int)identifier];
	}

	public override void DropItem(InventorySlot inventorySlot) {
        InventoryItem inventoryItem = inventorySlot.GetItemInSlot();
        ItemInstance itemInstance = inventoryItem.itemInstance;
        inventorySlot.RemoveItemFromSlot();
		if (OnItemDropped != null)
			OnItemDropped(itemInstance);
	}

	void OnValidate() {
		for (int i = 0; i < startItems.Length; i++) {
			if (startItems[i].quantity < 1)
				startItems[i].quantity = 1;
		}
	}
}
