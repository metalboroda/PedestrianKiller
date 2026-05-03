using Assets.Scripts.EventBus;
using Lean.Pool;
using UnityEngine;

namespace Assets.Scripts.Character
{
    public class CharacterDestroyer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float destroyTime = 10f;

        [Header("References")]
        [SerializeField] private GameObject pedestrianGameObject;

        private EventBinding<Events.CharacterIsDead> _characterIsDead;

        private void OnEnable()
        {
            _characterIsDead = new EventBinding<Events.CharacterIsDead>(OnCharacterIsDead);
            EventBus<Events.CharacterIsDead>.Register(_characterIsDead);
        }

        private void OnDisable()
        {
            EventBus<Events.CharacterIsDead>.Unregister(_characterIsDead);
        }

        private void OnCharacterIsDead(Events.CharacterIsDead eventData)
        {
            if (eventData.CharacterID == pedestrianGameObject.GetInstanceID())
            {
                LeanPool.Despawn(gameObject, destroyTime);
            }
        }
    }
}