using System.Linq;
using Assets.Scripts.EventBus;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects.Weapons;
using Lean.Pool;
using UnityEngine;

namespace Assets.Scripts.Test
{
    public class MouseDamageCaster : MonoBehaviour
    {
        private EventBinding<Events.FireRequested> _fireBinding;

        private void OnEnable()
        {
            _fireBinding = new EventBinding<Events.FireRequested>(HandleFireRequest);
            EventBus<Events.FireRequested>.Register(_fireBinding);
        }

        private void OnDisable()
        {
            EventBus<Events.FireRequested>.Unregister(_fireBinding);
        }

        private void HandleFireRequest(Events.FireRequested eventData)
        {
            WeaponConfigSo config = eventData.Config;
            Ray ray = eventData.ShootingRay;
            
            if (Physics.Raycast(ray, out RaycastHit damageHit, config.maxDistance, config.damageableLayerMask))
            {
                if (damageHit.collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(config.baseDamage, damageHit.point, damageHit.normal,
                        damageHit.collider.gameObject, config.shouldSpawnDecals);
                }
                
                Ray pierceRay = new Ray(damageHit.point + ray.direction * 0.01f, ray.direction);
                
                if (Physics.Raycast(pierceRay, out RaycastHit wallHit, config.maxDistance - damageHit.distance, config.vfxLayerMask))
                {
                    if (config.wallBloodPrefabs != null && config.wallBloodPrefabs.Length > 0)
                    {
                        GameObject randomBlood = config.wallBloodPrefabs[Random.Range(0, config.wallBloodPrefabs.Length)];
                        
                        if (randomBlood)
                            LeanPool.Spawn(randomBlood, wallHit.point, Quaternion.LookRotation(wallHit.normal));
                    }
                }
            }
            
            if (Physics.Raycast(ray, out RaycastHit physicsHit, config.maxDistance, config.physicalLayerMask))
            {
                Rigidbody rb = physicsHit.collider.attachedRigidbody;
                
                if (rb)
                    rb.AddForceAtPosition(ray.direction * config.forceAmount, physicsHit.point, config.forceMode);
            }
            
            if (Physics.Raycast(ray, out RaycastHit vfxHit, config.maxDistance, config.vfxLayerMask))
                SpawnMaterialEffect(vfxHit, config);
        }

        private void SpawnMaterialEffect(RaycastHit hit, WeaponConfigSo config)
        {
            Renderer targetRenderer = hit.collider.GetComponent<Renderer>();
            
            if (!targetRenderer) return;

            Material hitMaterial = targetRenderer.sharedMaterial;
            var effectConfig = config.materialEffects.FirstOrDefault(c => c.materials.Contains(hitMaterial));

            if (effectConfig != null && effectConfig.effectPrefabs != null && effectConfig.effectPrefabs.Length > 0)
            {
                GameObject randomEffect = effectConfig.effectPrefabs[Random.Range(0, effectConfig.effectPrefabs.Length)];
                
                if (randomEffect)
                    LeanPool.Spawn(randomEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }
}