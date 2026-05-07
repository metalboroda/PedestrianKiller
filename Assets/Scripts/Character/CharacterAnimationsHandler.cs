using Assets.Scripts.ScriptableObjects.Character;
using UnityEngine;

namespace Assets.Scripts.Character
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimationsHandler : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CharacterAnimationsDataSo characterAnimationsData;

        [Header("Settings")]
        [SerializeField] private float crossFadeDuration = 0.25f;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void IdleAnimation()
        {
            _animator.CrossFadeInFixedTime(characterAnimationsData.GetRandomIdleAnimation(), crossFadeDuration);
        }

        public void WalkAnimation()
        {
            _animator.CrossFadeInFixedTime(characterAnimationsData.GetRandomWalkAnimation(), crossFadeDuration);
        }

        public void InjuredWalkAnimation()
        {
            _animator.CrossFadeInFixedTime(characterAnimationsData.GetRandomInjuredWalkAnimation(), crossFadeDuration);
        }

        public void InjuredRunAnimation()
        {
            _animator.CrossFadeInFixedTime(characterAnimationsData.GetRandomInjuredRunAnimation(), crossFadeDuration);
        }

        public void CrouchAfraidAnimation()
        {
            _animator.CrossFadeInFixedTime(characterAnimationsData.GetRandomCrouchAfraidAnimation(), crossFadeDuration);
        }

        public void HitAnimation()
        {
            _animator.CrossFadeInFixedTime(characterAnimationsData.GetRandomHitAnimation(), crossFadeDuration);
        }

        public void InjuryAnimation()
        {
            _animator.CrossFadeInFixedTime(characterAnimationsData.GetRandomInjuredLyingAnimation(), crossFadeDuration);
        }

        public void DeathAnimation()
        {
            _animator.CrossFadeInFixedTime(characterAnimationsData.GetRandomDeathAnimation(), crossFadeDuration);
        }

        public void StopAnimator()
        {
            _animator.enabled = false;
        }
    }
}