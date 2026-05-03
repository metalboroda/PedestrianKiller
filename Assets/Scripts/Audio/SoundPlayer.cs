using Assets.Scripts.Components;
using Assets.Scripts.EventBus;
using Assets.Scripts.GameManagement.GameStates;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : MonoBehaviour
    {
        [Header("Clips")]
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;

        private AudioPlayerComponent _audioPlayerComponent;

        private EventBinding<Events.GameStateChanged> _gameStateChanged;
        private EventBinding<Events.UIButtonClicked> _uiButtonClicked;

        private void Awake()
        {
            _audioPlayerComponent = new AudioPlayerComponent()
                .SetAudioSource(GetComponent<AudioSource>())
                .SetMonoBehaviour(this);
        }

        private void OnEnable()
        {
            _gameStateChanged = new EventBinding<Events.GameStateChanged>(OnGameStateChanged);
            EventBus<Events.GameStateChanged>.Register(_gameStateChanged);
            _uiButtonClicked = new EventBinding<Events.UIButtonClicked>(OnUiButtonClicked);
            EventBus<Events.UIButtonClicked>.Register(_uiButtonClicked);
        }

        private void OnDisable()
        {
            EventBus<Events.GameStateChanged>.Unregister(_gameStateChanged);
            EventBus<Events.UIButtonClicked>.Unregister(_uiButtonClicked);
        }

        private void OnGameStateChanged(Events.GameStateChanged gameStateChanged)
        {
            switch (gameStateChanged.State)
            {
                case GameWinState:
                    _audioPlayerComponent.PlayClip(winClip, false, true);
                    break;
                case GameLoseState:
                    _audioPlayerComponent.PlayClip(loseClip, false, true);
                    break;
            }
        }

        private void OnUiButtonClicked()
        {
            _audioPlayerComponent.PlayClip(uiClickClip, false, true);
        }
    }
}