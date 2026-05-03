using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.SaveSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Canvases
{
    public class SettingsCanvasHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button backButton;
        [Space(20)]
        [SerializeField] private Button musicButton;
        [SerializeField] private GameObject musicOn;
        [SerializeField] private GameObject musicOff;
        [Space(20)]
        [SerializeField] private Button sfxButton;
        [SerializeField] private GameObject sfxOn;
        [SerializeField] private GameObject sfxOff;

        private SettingsSave _settings;

        private void OnEnable()
        {
            LoadSettings();

            backButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Back
                });
            });

            musicButton.onClick.AddListener(ToggleMusic);
            sfxButton.onClick.AddListener(ToggleSfx);
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
            musicButton.onClick.RemoveAllListeners();
            sfxButton.onClick.RemoveAllListeners();
        }

        private void LoadSettings()
        {
            _settings = SaveManager.LoadSettings();

            UpdateMusicButton();
            UpdateSfxButton();
        }

        private void ToggleMusic()
        {
            _settings.SaveMusic(!_settings.isMusicOn);

            EventBus<Events.UiMusicClicked>.Raise(new Events.UiMusicClicked());
            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());

            SaveManager.SaveSettings(_settings);

            UpdateMusicButton();
        }

        private void ToggleSfx()
        {
            _settings.SaveSfx(!_settings.isSfxOn);

            EventBus<Events.UiSfxClicked>.Raise(new Events.UiSfxClicked());
            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());

            SaveManager.SaveSettings(_settings);

            UpdateSfxButton();
        }

        private void UpdateMusicButton()
        {
            musicOn.SetActive(_settings.isMusicOn);
            musicOff.SetActive(!_settings.isMusicOn);
        }

        private void UpdateSfxButton()
        {
            sfxOn.SetActive(_settings.isSfxOn);
            sfxOff.SetActive(!_settings.isSfxOn);
        }
    }
}