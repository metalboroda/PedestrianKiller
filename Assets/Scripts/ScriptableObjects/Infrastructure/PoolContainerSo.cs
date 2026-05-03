using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Infrastructure
{
    [CreateAssetMenu(fileName = "PoolContainer", menuName = "ScriptableObjects/Infrastructure/PoolContainer")]
    public class PoolContainerSo : ScriptableObject
    {
        public Transform PoolContainer { get; set; }
    }
}