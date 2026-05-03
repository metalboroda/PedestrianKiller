using UnityEngine;

namespace Assets.Scripts.LevelLogic
{
    [CreateAssetMenu(fileName = "PedestrianData", menuName = "ScriptableObjects/LevelLogic/PedestrianData")]
    public class PedestrianDataSo : ScriptableObject
    {
        public int TotalSpawnedCount { get; private set; }

        public void Increment()
        {
            TotalSpawnedCount++;
        }

        public void ResetData()
        {
            TotalSpawnedCount = 0;
        }
    }
}