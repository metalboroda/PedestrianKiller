using System.Collections;
using Assets.Scripts.EventBus;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class TutorialItemAudioHandler : MonoBehaviour
    {
        [SerializeField] private AudioClip audioClip;

        private AudioSource _audioSource;

        private void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();

            if (audioClip)
            {
                _audioSource.clip = audioClip;
                _audioSource.Play();

                RaiseVoiceoverPlayedEvent(true);
                StartCoroutine(WaitForClipToEnd());
            }
        }

        private void OnDisable()
        {
            RaiseVoiceoverPlayedEvent(false);
            StopAllCoroutines();
        }

        private IEnumerator WaitForClipToEnd()
        {
            float waitTime = audioClip.length;
            float elapsed = 0f;

            while (elapsed < waitTime)
            {
                yield return null;

                elapsed += Time.deltaTime;

                if (!_audioSource.isPlaying)
                {
                    RaiseVoiceoverPlayedEvent(false);
                    break;
                }
            }

            RaiseVoiceoverPlayedEvent(false);
        }

        private void RaiseVoiceoverPlayedEvent(bool isVoiceoverPlayed)
        {
            EventBus<Events.VoiceoverPlayed>.Raise(new Events.VoiceoverPlayed
            {
                IsVoiceoverPlayed = isVoiceoverPlayed
            });
        }
    }
}