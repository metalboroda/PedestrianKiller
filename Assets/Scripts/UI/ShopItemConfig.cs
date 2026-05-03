using System;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [Serializable]
    public class ShopItemConfig
    {
        [field: Header("Settings")]
        [field: SerializeField] public string Name { get; private set; } = "Player_1";
        [field: SerializeField] public int Price { get; private set; }
        [field: SerializeField] public bool Unlocked { get; private set; }
        [field: SerializeField] public bool Selected { get; private set; }
        [field: SerializeField] public bool UseImageInsteadOfPrefab { get; private set; }

        [field: Header("References")]
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public Sprite ItemImage { get; private set; }
        [field: Space]
        [field: SerializeField] public ShopItemHandler ShopItemHandler { get; private set; }

        public void SetUnlocked(bool unlocked)
        {
            Unlocked = unlocked;
        }

        public void SetSelected(bool selected)
        {
            Selected = selected;
        }
    }
}