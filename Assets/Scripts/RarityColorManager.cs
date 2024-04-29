using UnityEngine;

public class RarityColorManager : MonoBehaviour
{
    public static RarityColorManager Instance { get; private set; }

    // Define your list of colors in the inspector
    [SerializeField] Color[] rarityColors;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Color GetColorByRarity(Rarity rarity)
    {
        return rarityColors[(int)rarity];
    }

	public Color GetBrighterColorByRarity(Rarity rarity) {
        // Make the color brighter white
        return rarityColors[(int)rarity] + new Color(0.4f, 0.4f, 0.4f, 0);
	}
	public Color GetDullerColorByRarity(Rarity rarity) {
		return rarityColors[(int)rarity] - new Color(0.4f, 0.4f, 0.4f, 0);
	}
}