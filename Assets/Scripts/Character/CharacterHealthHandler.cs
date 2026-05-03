using System.Collections;
using Assets.Scripts.EventBus;
using UnityEngine;

namespace Assets.Scripts.Character
{
    public class CharacterHealthHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float health = 100f;
        [SerializeField] private float injuredThreshold = 25f;

        private float _currentHealth;
        private bool _isInjured;
        private bool _isDead;
        private bool _canTakeDamage = true;
        private const float _canTakeDamageDelay = 1f;

        private EventBinding<Events.BodyPartDamaged> _bodyPartDamaged;

        private void Awake()
        {
            _currentHealth = health;
        }

        private void OnEnable()
        {
            _bodyPartDamaged = new EventBinding<Events.BodyPartDamaged>(OnBodyPartDamaged);
            EventBus<Events.BodyPartDamaged>.Register(_bodyPartDamaged);
        }

        private void OnDisable()
        {
            EventBus<Events.BodyPartDamaged>.Unregister(_bodyPartDamaged);
        }

        private void OnBodyPartDamaged(Events.BodyPartDamaged eventData)
        {
            if (eventData.CharacterID != gameObject.GetInstanceID()) return;

            float actualDamage = eventData.BaseDamage * eventData.DamageMultiplier;

            _currentHealth -= actualDamage;

            if (!_canTakeDamage) return;
            if (_currentHealth <= 0 && !_isDead)
            {
                _currentHealth = 0;

                Die();
            }

            if (_currentHealth <= injuredThreshold && !_isInjured && !_isDead)
            {
                FallInjured();

                _canTakeDamage = false;
            }
        }

        private void FallInjured()
        {
            EventBus<Events.CharacterIsInjured>.Raise(new Events.CharacterIsInjured
            {
                CharacterID = gameObject.GetInstanceID()
            });

            StartCoroutine(DoAllowToTakeDamage());
        }

        private void Die()
        {
            EventBus<Events.CharacterIsDead>.Raise(new Events.CharacterIsDead
            {
                CharacterID = gameObject.GetInstanceID()
            });

            _isDead = true;
        }

        private IEnumerator DoAllowToTakeDamage()
        {
            yield return new WaitForSeconds(_canTakeDamageDelay);

            _canTakeDamage = true;
        }
    }
}