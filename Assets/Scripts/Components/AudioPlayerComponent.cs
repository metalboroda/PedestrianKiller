using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Components
{
    public class AudioPlayerComponent
    {
        private Coroutine _currentCoroutine;
        private AudioSource _audioSource;
        private MonoBehaviour _monoBehaviour;

        public AudioPlayerComponent SetAudioSource(AudioSource audioSource)
        {
            _audioSource = audioSource;
            return this;
        }

        public AudioPlayerComponent SetMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            _monoBehaviour = monoBehaviour;
            return this;
        }

        public void PlayClip(AudioClip clip, bool oneShot = false, bool randomPitch = false, float? delay = null)
        {
            if (!clip || !_audioSource) return;
            if (delay.HasValue)
            {
                if (!_monoBehaviour) return;

                StopCurrentTask();

                _currentCoroutine = _monoBehaviour.StartCoroutine(PlayWithDelay(clip, delay.Value, oneShot, randomPitch));
            }
            else
            {
                if (randomPitch) SetRandomPitch();

                PlayAudioClip(clip, oneShot);
            }
        }

        public void PlayClips(AudioClip[] clips, bool oneShot = false, bool randomPitch = false, float? delay = null)
        {
            if (clips.Length == 0 || !_audioSource) return;
            if (delay.HasValue)
            {
                if (!_monoBehaviour) return;

                StopCurrentTask();

                _currentCoroutine = _monoBehaviour.StartCoroutine(PlayMultipleWithDelay(clips, delay.Value, oneShot, randomPitch));
            }
            else
            {
                foreach (AudioClip clip in clips)
                {
                    if (clip)
                    {
                        if (randomPitch) SetRandomPitch();

                        PlayAudioClip(clip, oneShot);
                    }
                }
            }
        }

        public void PlayRandomClip(AudioClip[] clips, bool oneShot = false, bool randomPitch = false, float? delay = null)
        {
            if (clips.Length == 0 || !_audioSource) return;

            AudioClip randomClip = clips[Random.Range(0, clips.Length)];

            PlayClip(randomClip, oneShot, randomPitch, delay);
        }

        public void StopAll()
        {
            StopCurrentTask();

            if (_audioSource?.isPlaying == true)
            {
                _audioSource.Stop();
            }
        }

        private void StopCurrentTask()
        {
            if (_currentCoroutine != null && _monoBehaviour != null)
            {
                _monoBehaviour.StopCoroutine(_currentCoroutine);
                
                _currentCoroutine = null;
            }
        }

        private void SetRandomPitch(float minPitch = 0.95f, float maxPitch = 1.05f)
        {
            if (_audioSource)
            {
                _audioSource.pitch = Random.Range(minPitch, maxPitch);
            }
        }

        private void PlayAudioClip(AudioClip clip, bool oneShot)
        {
            if (oneShot)
            {
                _audioSource.PlayOneShot(clip);
            }
            else
            {
                _audioSource.clip = clip;
                _audioSource.Play();
            }
        }

        private IEnumerator PlayWithDelay(AudioClip clip, float delay, bool oneShot, bool randomPitch)
        {
            yield return new WaitForSeconds(delay);

            if (randomPitch) SetRandomPitch();

            PlayAudioClip(clip, oneShot);

            _currentCoroutine = null;
        }

        private IEnumerator PlayMultipleWithDelay(AudioClip[] clips, float delay, bool oneShot, bool randomPitch)
        {
            yield return new WaitForSeconds(delay);

            foreach (AudioClip clip in clips)
            {
                if (clip)
                {
                    if (randomPitch) SetRandomPitch();

                    PlayAudioClip(clip, oneShot);
                }
            }

            _currentCoroutine = null;
        }
    }
}