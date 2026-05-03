using System.Collections;
using Assets.Scripts.EventBus;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundtrackPlayer : MonoBehaviour
    {
        [Header("Reducing By Voice")]
        [SerializeField] private float reducedVolume = 0.2f;
        [SerializeField] private float volumeChangeDuration = 0.125f;

        [Header("Clips")]
        [SerializeField] private AudioClip[] soundtrackClips;

        [Header("References")]
        [SerializeField] private AudioSource audioSource;

        private float _initVolume;
        private Coroutine _volumeChangeCoroutine;

        private EventBinding<Events.VoiceoverPlayed> _voiceoverPlayed;

        private void OnEnable()
        {
            _voiceoverPlayed = new EventBinding<Events.VoiceoverPlayed>(OnVoiceoverPlayed);
            EventBus<Events.VoiceoverPlayed>.Register(_voiceoverPlayed);
        }

        private void OnDisable()
        {
            EventBus<Events.VoiceoverPlayed>.Unregister(_voiceoverPlayed);
            _voiceoverPlayed = null;
        }

        private void Start()
        {
            if (!audioSource)
            {
                Debug.LogError("Audio Source must be assigned!");
                enabled = false;
                return;
            }

            _initVolume = audioSource.volume;

            StartCoroutine(DoPlaySoundtracks());
        }

        private IEnumerator DoPlaySoundtracks()
        {
            if (soundtrackClips == null || soundtrackClips.Length == 0)
                yield break;

            while (true)
            {
                int startIndex = Random.Range(0, soundtrackClips.Length);

                for (int i = 0; i < soundtrackClips.Length; i++)
                {
                    int currentClipIndex = (startIndex + i) % soundtrackClips.Length;

                    AudioClip clipToPlay = soundtrackClips[currentClipIndex];

                    if (!clipToPlay) continue;

                    audioSource.clip = clipToPlay;
                    audioSource.Play();

                    yield return new WaitWhile(() => audioSource.isPlaying);
                }
            }
        }

        private void OnVoiceoverPlayed(Events.VoiceoverPlayed voiceoverPlayed)
        {
            if (_volumeChangeCoroutine != null)
            {
                StopCoroutine(_volumeChangeCoroutine);
            }

            float targetVolume = voiceoverPlayed.IsVoiceoverPlayed ? reducedVolume : _initVolume;

            if (audioSource && gameObject.activeInHierarchy)
            {
                _volumeChangeCoroutine = StartCoroutine(ChangeVolume(targetVolume, volumeChangeDuration));
            }
        }

        private IEnumerator ChangeVolume(float targetVolume, float duration)
        {
            float currentTime = 0;
            float startVolume = audioSource.volume;

            while (currentTime < duration)
            {
                currentTime += Time.unscaledDeltaTime;

                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
                yield return null;
            }

            audioSource.volume = targetVolume;

            _volumeChangeCoroutine = null;
        }
    }
}