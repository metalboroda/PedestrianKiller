using System;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [Serializable]
    public class LevelSelectorItemConfig
    {
        [field: Header("Settings")]
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int Rating { get; private set; }
        [field: SerializeField] public bool Unlocked { get; private set; }

        [field: Header("References")]
        [field: SerializeField] public GameObject LevelSelectorItemPrefab { get; private set; }

        public void SetRating(int rating)
        {
            Rating = rating;
        }

        public void SetUnlocked(bool unlocked)
        {
            Unlocked = unlocked;
        }
    }
}