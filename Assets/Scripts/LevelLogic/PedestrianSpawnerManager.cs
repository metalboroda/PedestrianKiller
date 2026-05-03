using Assets.Scripts.EventBus;
using UnityEngine;

namespace Assets.Scripts.LevelLogic
{
    public class PedestrianSpawnerManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PedestrianDataSo pedestrianData;

        private int _overallMaxSpawns;

        private EventBinding<Events.SpawnerInitialized> _spawnerInitialized;

        private void OnEnable()
        {
            pedestrianData.ResetData();

            _spawnerInitialized = new EventBinding<Events.SpawnerInitialized>(OnSpawnerInitialized);
            EventBus<Events.SpawnerInitialized>.Register(_spawnerInitialized);
        }

        private void OnDisable()
        {
            EventBus<Events.SpawnerInitialized>.Unregister(_spawnerInitialized);
        }

        private void OnSpawnerInitialized(Events.SpawnerInitialized eventData)
        {
            _overallMaxSpawns += eventData.NumberOfSpawns;
        }
    }
}