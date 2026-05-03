using Assets.Scripts.EventBus;
using Assets.Scripts.Hashes;
using Assets.Scripts.SaveSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Scripts.GameManagement
{
    public class AudioManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioMixer mixer;

        private const float MaxVolume = 0f;
        private const float MinVolume = -80f;

        private SettingsSave _gameSettings;

        private EventBinding<Events.UiMusicClicked> _uiMusicClicked;
        private EventBinding<Events.UiSfxClicked> _uiSfxClicked;

        private void OnEnable()
        {
            _uiMusicClicked = new EventBinding<Events.UiMusicClicked>(SwitchMusicVolume);
            EventBus<Events.UiMusicClicked>.Register(_uiMusicClicked);
            _uiSfxClicked = new EventBinding<Events.UiSfxClicked>(SwitchSfxVolume);
            EventBus<Events.UiSfxClicked>.Register(_uiSfxClicked);
        }

        private void OnDisable()
        {
            EventBus<Events.UiMusicClicked>.Unregister(_uiMusicClicked);
            _uiMusicClicked = null;
            EventBus<Events.UiSfxClicked>.Unregister(_uiSfxClicked);
            _uiSfxClicked = null;
        }

        private void Start()
        {
            LoadSettings();
            ApplyVolumeSettings();
        }

        private void LoadSettings()
        {
            _gameSettings = SaveManager.LoadSettings() ?? new SettingsSave();
        }

        private void ApplyVolumeSettings()
        {
            mixer.SetFloat(SettingsHashes.MusicVolume, _gameSettings.isMusicOn ? MaxVolume : MinVolume);
            mixer.SetFloat(SettingsHashes.SfxVolume, _gameSettings.isSfxOn ? MaxVolume : MinVolume);
        }

        private void SaveSettings()
        {
            SaveManager.SaveSettings(_gameSettings);
        }

        private void SwitchMusicVolume()
        {
            _gameSettings.SaveMusic(_gameSettings.isMusicOn == false);

            mixer.SetFloat(SettingsHashes.MusicVolume, _gameSettings.isMusicOn ? MaxVolume : MinVolume);

            SaveSettings();
        }

        private void SwitchSfxVolume()
        {
            _gameSettings.SaveSfx(_gameSettings.isSfxOn == false);

            mixer.SetFloat(SettingsHashes.SfxVolume, _gameSettings.isSfxOn ? MaxVolume : MinVolume);

            SaveSettings();
        }
    }
}