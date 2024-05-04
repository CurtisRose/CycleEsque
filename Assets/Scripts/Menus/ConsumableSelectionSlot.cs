using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableSelectionSlot : MonoBehaviour
{
	[SerializeField] public TMP_Text stackSizeText;
	[SerializeField] public Image itemImage;
	[SerializeField] Image itemBackgroundImage;
	[SerializeField] Image itemBorderImage;

	public void SetImageColor(Rarity rarity) {
		Color temp = RarityColorManager.Instance.GetColorByRarity(rarity);
		itemBackgroundImage.color = temp;
		itemBorderImage.color = temp;
	}
}
