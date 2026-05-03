using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Canvases
{
    public class PauseCanvasHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button settingsButton;

        private void OnEnable()
        {
            resumeButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.ResumeGame,
                });
            });

            restartButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Restart,
                });
            });

            mainMenuButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.MainMenu,
                });
            });

            settingsButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Settings,
                });
            });
        }

        private void OnDisable()
        {
            resumeButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
        }
    }
}