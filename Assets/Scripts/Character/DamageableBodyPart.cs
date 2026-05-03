using Assets.Scripts.EventBus;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Character
{
    public class DamageableBodyPart : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] private BodyPartType bodyPartType = BodyPartType.Default;
        [SerializeField] private float damageMultiplier = 1f;
        
        [Header("References")]
        [SerializeField] private GameObject parentObject;

        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, GameObject hitObject, bool spawnDecal = true)
        {
            int characterId = parentObject ? parentObject.GetInstanceID() : gameObject.GetInstanceID();

            EventBus<Events.BodyPartDamaged>.Raise(new Events.BodyPartDamaged
            {
                BaseDamage = damage,
                BodyPartType = bodyPartType,
                DamageMultiplier = damageMultiplier,
                CharacterID = characterId,
                
                HitPoint = hitPoint,
                HitNormal = hitNormal,
                HitObject = gameObject,
                
                SpawnDecal = spawnDecal
            });
        }
    }
}