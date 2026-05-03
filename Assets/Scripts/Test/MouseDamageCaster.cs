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
                    damageable.TakeDamage(config.baseDamage, damageHit.point, damageHit.normal,
                        damageHit.collider.gameObject, config.shouldSpawnDecals);
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

            if (effectConfig != null && effectConfig.effectPrefab)
                LeanPool.Spawn(effectConfig.effectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}