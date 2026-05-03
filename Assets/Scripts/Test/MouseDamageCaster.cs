using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Test
{
    public class MouseDamageCaster : MonoBehaviour
    {
        [Header("Distance")]
        [SerializeField] private float maxDistance = 100f;

        [Header("Damage Settings")]
        [SerializeField] private float baseDamage = 15f;
        [Space]
        [SerializeField] private LayerMask damageableLayerMask;

        [Header("Physics Settings")]
        [SerializeField] private float forceAmount = 10f;
        [Space]
        [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
        [Space]
        [SerializeField] private LayerMask physicalLayerMask;

        [Header("Decals Settings")]
        [SerializeField] private bool shouldSpawnDecals = true;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                HandleMouseClick();
        }

        private void HandleMouseClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit damageHit, maxDistance, damageableLayerMask))
            {
                if (damageHit.collider.TryGetComponent(out IDamageable damageable))
                    damageable.TakeDamage(baseDamage, damageHit.point, damageHit.normal, damageHit.collider.gameObject, shouldSpawnDecals);
            }
            
            if (Physics.Raycast(ray, out RaycastHit physicsHit, maxDistance, physicalLayerMask))
            {
                Rigidbody rb = physicsHit.collider.attachedRigidbody;
                
                if (rb)
                    rb.AddForceAtPosition(ray.direction * forceAmount, physicsHit.point, forceMode);
            }
        }
    }
}