using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Character
{
    [CreateAssetMenu(fileName = "CharacterAnimationsDataSo", menuName = "ScriptableObjects/Character/CharacterAnimationsDataSo")]
    public class CharacterAnimationsDataSo : ScriptableObject
    {
        [Header("Idle Animations")]
        [SerializeField] private string[] idleAnimations;

        [field: Header("Movement Animations")]
        [field: SerializeField] public string[] WalkAnimations { get; private set; }
        [field: SerializeField] public string[] InjuredWalkAnimations { get; private set; }
        [field: SerializeField] public string[] InjuredRunAnimations { get; private set; }

        [Header("Hit Animations")]
        [SerializeField] private string[] hitAnimations;

        [field: Header("Injury Animations")]
        [field: SerializeField] public string[] InjuredLyingAnimations { get; private set; }

        [Header("Death Animations")]
        [SerializeField] private string[] deathAnimations;

        public string GetRandomIdleAnimation()
        {
            return GetRandomAnimation(idleAnimations);
        }

        public string GetRandomWalkAnimation()
        {
            return GetRandomAnimation(WalkAnimations);
        }

        public string GetRandomInjuredWalkAnimation()
        {
            return GetRandomAnimation(InjuredWalkAnimations);
        }

        public string GetRandomInjuredRunAnimation()
        {
            return GetRandomAnimation(InjuredRunAnimations);
        }

        public string GetRandomInjuredLyingAnimation()
        {
            return GetRandomAnimation(InjuredLyingAnimations);
        }

        public string GetRandomHitAnimation()
        {
            return GetRandomAnimation(hitAnimations);
        }

        public string GetRandomDeathAnimation()
        {
            return GetRandomAnimation(deathAnimations);
        }

        private string GetRandomAnimation(string[] animations)
        {
            return animations[Random.Range(0, animations.Length)];
        }
    }
}