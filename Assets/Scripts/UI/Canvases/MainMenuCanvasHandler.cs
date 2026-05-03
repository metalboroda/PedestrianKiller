using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Canvases
{
    public class MainMenuCanvasHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button infoButton;

        private void OnEnable()
        {
            startButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.LevelSelector
                });
            });

            settingsButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Settings
                });
            });

            shopButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Shop
                });
            });

            infoButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Info
                });
            });
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            shopButton.onClick.RemoveAllListeners();
            infoButton.onClick.RemoveAllListeners();
        }
    }
}