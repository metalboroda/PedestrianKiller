using Assets.Scripts.Components;
using Lean.Pool;
using UnityEngine;

namespace Assets.Scripts.VFX
{
    [RequireComponent(typeof(ParticleSystem))] [RequireComponent(typeof(AudioSource))]
    public class VFXPrefabHandler : MonoBehaviour, IPoolable
    {
        [SerializeField] private float destroyTime = 3f;

        [Header("Audio")]
        [SerializeField] private bool needSound = true;
        [Space]
        [SerializeField] private AudioClip[] audioClips;

        private AudioPlayerComponent _audioPlayerComponent;

        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();

            if (needSound)
            {
                _audioPlayerComponent = new AudioPlayerComponent()
                    .SetAudioSource(GetComponent<AudioSource>())
                    .SetMonoBehaviour(this);
            }
        }

        public void OnSpawn()
        {
            _particleSystem.Play();

            if (needSound)
            {
                _audioPlayerComponent.PlayRandomClip(audioClips, false, true);
            }

            LeanPool.Despawn(gameObject, destroyTime);
        }

        public void OnDespawn()
        {
            _particleSystem.Stop();

            if (needSound)
            {
                _audioPlayerComponent.StopAll();
            }
        }
    }
}