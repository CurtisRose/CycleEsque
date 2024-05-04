using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsumableSelectionSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler {
	[SerializeField] public TMP_Text stackSizeText;
	[SerializeField] public Image itemImage;
	[SerializeField] public SharedItemData itemData;
	[SerializeField] Image itemBackgroundImage;
	[SerializeField] Image itemBorderImage;

	public static ConsumableSelectionSlot CurrentHoveredConsumable { get; private set; }

	public void Initialize(SharedItemData itemData) {
		this.itemData = itemData;
		itemImage.sprite = itemData.SmallImage;
		SetImageColor(itemData.Rarity);
	}

	// Maybe this one closes the context menu AND selects the consumable. Maybe it uses it also.
	public void OnPointerClick(PointerEventData eventData) {
		CurrentHoveredConsumable = this;
		ConsumableSelectionMenu.Instance.Close();
	}

	// Selects this consumable in the context menu
	public void OnPointerEnter(PointerEventData eventData) {
		CurrentHoveredConsumable = this;
	}

	private void SetImageColor(Rarity rarity) {
		Color temp = RarityColorManager.Instance.GetColorByRarity(rarity);
		itemBackgroundImage.color = temp;
		itemBorderImage.color = temp;
	}
}
