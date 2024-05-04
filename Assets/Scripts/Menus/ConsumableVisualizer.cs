using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConsumableVisualizer : MonoBehaviour
{
	public static ConsumableVisualizer Instance;

	InventoryItemTracker itemTracker;
	[SerializeField] private TMP_Text numberAvailableText;
	[SerializeField] private SharedItemData injectorData;
	[SerializeField] private Image image;
	[SerializeField] private Image border;
	[SerializeField] private Image background;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(gameObject);
		}

		itemTracker = GetComponent<InventoryItemTracker>();
		itemTracker.OnNumberOfItemsChanged += NumberOfItemsChanged;
		image.sprite = injectorData.SmallImage;
	}

	private void Start() {
		border.color = RarityColorManager.Instance.GetBrighterColorByRarity(injectorData.Rarity);
		background.color = RarityColorManager.Instance.GetDullerColorByRarity(injectorData.Rarity);
	}

	private void NumberOfItemsChanged(int numberOfItems) {
		numberAvailableText.text = numberOfItems.ToString();
	}

	public void SetInjectorData(SharedItemData data) {
		injectorData = data;
		image.sprite = injectorData.SmallImage;
		border.color = RarityColorManager.Instance.GetBrighterColorByRarity(injectorData.Rarity);
		background.color = RarityColorManager.Instance.GetDullerColorByRarity(injectorData.Rarity);
		itemTracker.SetNewItemToTrack(data);
	}
}
