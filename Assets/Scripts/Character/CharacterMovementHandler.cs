using UnityEngine;

namespace Assets.Scripts.Character
{
    public class CharacterMovementHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float injuredWalkSpeed = 1f;
        [SerializeField] private float injuredRunSpeed = 3f;

        public void WalkForward()
        {
            transform.Translate(Vector3.forward * (walkSpeed * Time.deltaTime));
        }
        
        public void InjuredWalkForward()
        {
            transform.Translate(Vector3.forward * (injuredWalkSpeed * Time.deltaTime));
        }

        public void InjuredRunForward()
        {
            transform.Translate(Vector3.forward * (injuredRunSpeed * Time.deltaTime));
        }

        public void StopMovement()
        {
            transform.Translate(Vector3.zero);
        }
    }
}