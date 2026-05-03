using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, GameObject hitObject, bool spawnDecal = true);
    }
}