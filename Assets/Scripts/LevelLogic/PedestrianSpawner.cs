using Assets.Scripts.EventBus;
using Lean.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.LevelLogic
{
    public class PedestrianSpawner : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PedestrianDataSo pedestrianData;

        [Header("Settings")]
        [SerializeField] private float minSpawnRate;
        [SerializeField] private float maxSpawnRate;
        [Space]
        [SerializeField] private int numberOfSpawns = 5;

        [Header("References")]
        [SerializeField] private GameObject pedestrianPrefab;

        private float _spawnTick;
        private float _randomTime;
        private int _localSpawnCount;

        private void Start()
        {
            EventBus<Events.SpawnerInitialized>.Raise(new Events.SpawnerInitialized
            {
                NumberOfSpawns = numberOfSpawns
            });

            _randomTime = Random.Range(minSpawnRate, maxSpawnRate);
        }

        private void Update()
        {
            if (_localSpawnCount >= numberOfSpawns) return;

            _spawnTick += Time.deltaTime;

            if (_spawnTick >= _randomTime)
            {
                SpawnPedestrian();
                
                _spawnTick = 0f;
                _randomTime = Random.Range(minSpawnRate, maxSpawnRate);
            }
        }

        private void SpawnPedestrian()
        {
            LeanPool.Spawn(pedestrianPrefab, transform.position, transform.rotation);

            _localSpawnCount++;
            
            pedestrianData.Increment();

            EventBus<Events.PedestrianSpawned>.Raise(new Events.PedestrianSpawned
            {
                TotalSpawnedCount = pedestrianData.TotalSpawnedCount
            });
        }
    }
}