using System;
using UnityEngine;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class LevelSave
    {
        [SerializeField] private int[] levelRatings = new int[99];

        public int GetLevelRating(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < levelRatings.Length)
            {
                return levelRatings[levelIndex];
            }
            return 0;
        }

        public void SetLevelRating(int levelIndex, int rating)
        {
            if (levelIndex < 0 || levelIndex >= levelRatings.Length) return;
            if (rating > levelRatings[levelIndex])
            {
                levelRatings[levelIndex] = rating;
            }
        }
    }
}