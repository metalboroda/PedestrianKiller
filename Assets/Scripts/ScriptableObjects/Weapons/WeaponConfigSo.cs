using System;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Weapons
{
    [CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "ScriptableObjects/WeaponConfig")]
    public class WeaponConfigSo : ScriptableObject
    {
        [Header("General Settings")]
        public string weaponName = "Pistol";
        [Space]
        public float fireRate = 0.2f;
        public float maxDistance = 100f;

        [Header("Ammo Settings")]
        public int magazineSize = 12;
        public float reloadDuration = 1.5f;

        [Header("Damage Settings")]
        public float baseDamage = 15f;
        [Space]
        public LayerMask damageableLayerMask;
        [Space]
        public bool shouldSpawnDecals = true;

        [Header("Physics Settings")]
        public float forceAmount = 10f;
        [Space]
        public ForceMode forceMode = ForceMode.Impulse;
        [Space]
        public LayerMask physicalLayerMask;

        [Header("Rotation Settings")]
        public float rotationSpeed = 20f;

        [Header("Recoil Settings (DOTween)")]
        [Tooltip("Сила віддачі назад")]
        public float recoilStrength = 0.1f;
        [Tooltip("Сила підкидання ствола")]
        public float recoilRotationStrength = 5f;
        [Tooltip("Тривалість анімації віддачі")]
        public float recoilDuration = 0.1f;
        [Tooltip("Вібрація (для Punch)")]
        public int recoilVibrato = 5;

        [Header("VFX Settings")]
        public LayerMask vfxLayerMask;
        [Space]
        public GameObject[] wallBloodPrefabs;

        [Space]
        public MaterialEffectConfig[] materialEffects;

        [Serializable]
        public class MaterialEffectConfig
        {
            public Material[] materials;
            public GameObject[] effectPrefabs;
        }
    }
}