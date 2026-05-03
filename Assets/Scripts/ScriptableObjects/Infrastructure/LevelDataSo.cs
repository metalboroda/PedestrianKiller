using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Infrastructure
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Infrastructure/LevelData", fileName = "LevelData")]
    public class LevelDataSo : ScriptableObject
    {
        public int CurrentLevelRating { get; set; }
        public Transform LevelTransform { get; set; }
    }
}