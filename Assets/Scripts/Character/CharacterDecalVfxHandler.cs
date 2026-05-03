using System.Collections;
using Assets.Scripts.EventBus;
using Assets.Scripts.VFX;
using Lean.Pool;
using UnityEngine;

namespace Assets.Scripts.Character
{
    public class CharacterDecalVfxHandler : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private bool canSpawnDecals = true;
        [SerializeField] private bool canSpawnImpactVfx = true;
        [SerializeField] private bool canSpawnPuddles = true;
        [SerializeField] private bool useRandomRotation = true;

        [Header("Impact VFX")]
        [SerializeField] private GameObject[] impactPrefabs;

        [Header("Blood Puddles (Ray Search)")]
        [SerializeField] private GameObject[] puddlePrefabs;
        [Space]
        [SerializeField] private float puddleSpawnDelay = 1.5f;
        [Space]
        [SerializeField] private Transform puddlePoint;
        [Space]
        [SerializeField] private float offsetY = 0.2f;
        [SerializeField] private float castDistance = 3f;
        [SerializeField] private float puddleOffsetFromGround;
        [Space]
        [SerializeField] private LayerMask groundLayerMask;

        [Header("Decal Settings")]
        [SerializeField] private DecalType[] bloodDecalTypes;
        [SerializeField] private float sizeX = 0.1f;
        [SerializeField] private float sizeY = 0.1f;
        [SerializeField] private float depth = 0.3f;
        [SerializeField] private float normalOffset = 0.2f;

        [Header("Decal Visuals")]
        [Range(0f, 1f)] [SerializeField] private float opacity = 1f;
        [Range(0f, 1f)] [SerializeField] private float angleClip = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showGizmos;

        private int _myCharacterId;
        private DecalSpawner[] _spawners;
        private Transform _lastHitTransform;
        private Vector3 _lastHitNormal = Vector3.up;

        private EventBinding<Events.BodyPartDamaged> _onHitBinding;
        private EventBinding<Events.CharacterIsDead> _onDeadBinding;

        private void Awake()
        {
            _myCharacterId = gameObject.GetInstanceID();
        }

        private void Start()
        {
            if (bloodDecalTypes != null && bloodDecalTypes.Length > 0)
            {
                _spawners = new DecalSpawner[bloodDecalTypes.Length];

                for (int i = 0; i < bloodDecalTypes.Length; i++)
                {
                    if (bloodDecalTypes[i])
                        _spawners[i] = DecalManager.GetSpawner(bloodDecalTypes[i].decalSettings, 20000, 2000);
                }
            }
        }

        private void OnEnable()
        {
            _onHitBinding = new EventBinding<Events.BodyPartDamaged>(HandleHit);
            EventBus<Events.BodyPartDamaged>.Register(_onHitBinding);
            _onDeadBinding = new EventBinding<Events.CharacterIsDead>(HandleCharacterDead);
            EventBus<Events.CharacterIsDead>.Register(_onDeadBinding);
        }

        private void OnDisable()
        {
            EventBus<Events.BodyPartDamaged>.Unregister(_onHitBinding);
            EventBus<Events.CharacterIsDead>.Unregister(_onDeadBinding);
        }

        private void HandleHit(Events.BodyPartDamaged data)
        {
            if (data.CharacterID != _myCharacterId) return;
            if (data.HitObject)
            {
                _lastHitTransform = data.HitObject.transform;
                _lastHitNormal = data.HitNormal;
            }

            if (canSpawnImpactVfx && data.SpawnDecal)
                SpawnImpactVfx(data);

            if (canSpawnDecals && data.SpawnDecal && _spawners != null)
                SpawnDecalAtPoint(data);
        }

        private void HandleCharacterDead(Events.CharacterIsDead data)
        {
            if (data.CharacterID != _myCharacterId) return;
            if (canSpawnPuddles)
                StartCoroutine(DoSpawnPuddleWithDelay());
        }

        private IEnumerator DoSpawnPuddleWithDelay()
        {
            yield return new WaitForSeconds(puddleSpawnDelay);

            SpawnBloodPuddle();
        }

        private void SpawnBloodPuddle()
        {
            Transform target = puddlePoint ? puddlePoint : _lastHitTransform;

            if (puddlePrefabs == null || puddlePrefabs.Length == 0 || !target) return;

            Vector3 rayOrigin = target.position + Vector3.up * offsetY;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, castDistance, groundLayerMask))
            {
                GameObject prefab = puddlePrefabs[Random.Range(0, puddlePrefabs.Length)];

                Vector3 forwardDirection = Vector3.ProjectOnPlane(_lastHitNormal, hit.normal).normalized;

                if (forwardDirection == Vector3.zero) forwardDirection = Vector3.forward;

                Quaternion rotation = Quaternion.LookRotation(forwardDirection, hit.normal);

                GameObject spawnedPuddle = LeanPool.Spawn(prefab, hit.point + hit.normal * puddleOffsetFromGround, rotation);

                BloodPuddleFollower follower = spawnedPuddle.GetComponentInChildren<BloodPuddleFollower>();

                if (follower)
                    follower.Init(target, groundLayerMask, castDistance, puddleOffsetFromGround, offsetY);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Transform target = puddlePoint ? puddlePoint : _lastHitTransform;

            if (!target) return;

            Vector3 rayOrigin = target.position + Vector3.up * offsetY;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rayOrigin, 0.05f);
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * castDistance);

            if (puddlePoint)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(puddlePoint.position, 0.1f);
            }
        }

        private void SpawnImpactVfx(Events.BodyPartDamaged data)
        {
            if (impactPrefabs == null || impactPrefabs.Length == 0) return;

            GameObject vfx = impactPrefabs[Random.Range(0, impactPrefabs.Length)];

            if (vfx) LeanPool.Spawn(vfx, data.HitPoint, Quaternion.LookRotation(data.HitNormal));
        }

        private void SpawnDecalAtPoint(Events.BodyPartDamaged data)
        {
            int randomIndex = Random.Range(0, _spawners.Length);
            DecalSpawner spawner = _spawners[randomIndex];

            if (spawner == null) return;

            Vector3 pos = data.HitPoint + data.HitNormal * normalOffset;
            Quaternion rot = Quaternion.LookRotation(-data.HitNormal);

            if (useRandomRotation) rot *= Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            spawner.AddDecal(pos, rot, data.HitObject, sizeX, sizeY, depth, opacity, angleClip, data.HitObject.transform);
        }
    }
}