using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Test
{
    public class MouseDamageCaster : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask damageableLayerMask;

        [Header("Physics Settings")]
        [SerializeField] private float forceAmount = 10f;
        [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

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

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, damageableLayerMask))
            {
                if (hit.collider.TryGetComponent(out IDamageable damageable))
                    damageable.TakeDamage(baseDamage, hit.point, hit.normal, hit.collider.gameObject, shouldSpawnDecals);

                Rigidbody rb = hit.collider.attachedRigidbody;

                if (rb)
                    rb.AddForceAtPosition(ray.direction * forceAmount, hit.point, forceMode);
            }
        }
    }
}