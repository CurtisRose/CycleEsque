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
}