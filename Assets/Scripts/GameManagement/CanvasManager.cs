using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.GameManagement.GameStates;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.UI.Canvases;
using UnityEngine;

namespace Assets.Scripts.GameManagement
{
    public class CanvasManager : MonoBehaviour
    {
        [Header("Canvases")]
        [SerializeField] private GameObject mainMenuCanvas;
        [SerializeField] private GameObject infoCanvas;
        [SerializeField] private GameObject settingsCanvas;
        [SerializeField] private GameObject shopCanvas;
        [SerializeField] private GameObject levelSelectorCanvas;
        [SerializeField] private GameObject gameCanvas;
        [SerializeField] private GameObject pauseCanvas;
        [SerializeField] private GameObject winCanvas;
        [SerializeField] private GameObject loseCanvas;
        [SerializeField] private GameObject tutorialCanvas;

        private readonly List<GameObject> _canvases = new();
        private GameObject _currentCanvas;
        private GameObject _previousCanvas;

        private EventBinding<Events.GameStateChanged> _gameStateChanged;
        private EventBinding<Events.UIButtonClicked> _uiButtonClicked;

        private void Awake()
        {
            AddCanvasesToList();

            tutorialCanvas.GetComponent<TutorialCanvasHandler>();
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
            _gameStateChanged = null;
            EventBus<Events.UIButtonClicked>.Unregister(_uiButtonClicked);
            _uiButtonClicked = null;
        }

        private void AddCanvasesToList()
        {
            _canvases.Add(mainMenuCanvas);
            _canvases.Add(infoCanvas);
            _canvases.Add(settingsCanvas);
            _canvases.Add(shopCanvas);
            _canvases.Add(levelSelectorCanvas);
            _canvases.Add(gameCanvas);
            _canvases.Add(pauseCanvas);
            _canvases.Add(winCanvas);
            _canvases.Add(loseCanvas);
            _canvases.Add(tutorialCanvas);

            foreach (GameObject canvas in _canvases)
            {
                canvas.SetActive(false);
            }
        }

        private void OnGameStateChanged(Events.GameStateChanged gameStateChanged)
        {
            switch (gameStateChanged.State)
            {
                case GameMainMenuState:
                    SwitchCanvas(mainMenuCanvas);
                    break;
                case GamePreviewState:
                    DisableAllCanvases();
                    break;
                case GamePlayState:
                    SwitchCanvas(gameCanvas);
                    EnableTutorialCanvas();
                    break;
                case GamePauseState:
                    SwitchCanvas(pauseCanvas);
                    break;
                case GameWinState:
                    SwitchCanvas(winCanvas);
                    break;
                case GameLoseState:
                    SwitchCanvas(loseCanvas);
                    break;
            }
        }

        private void OnUiButtonClicked(Events.UIButtonClicked uiButtonClicked)
        {
            switch (uiButtonClicked.ButtonType)
            {
                case UiButtonType.Default:
                    break;
                case UiButtonType.Info:
                    SwitchCanvas(infoCanvas);
                    break;
                case UiButtonType.Settings:
                    SwitchCanvas(settingsCanvas);
                    break;
                case UiButtonType.Shop:
                    SwitchCanvas(shopCanvas);
                    break;
                case UiButtonType.LevelSelector:
                    DisableAllCanvases();
                    SwitchCanvas(levelSelectorCanvas);
                    break;
                case UiButtonType.Back:
                    SwitchCanvas(_previousCanvas);
                    break;
                case UiButtonType.Restart:
                    DisableAllCanvases();
                    SwitchCanvas(gameCanvas);
                    break;
                case UiButtonType.NextLevel:
                    DisableAllCanvases();
                    SwitchCanvasWithDelay(levelSelectorCanvas, 0.01f);
                    break;
            }
        }

        private void EnableTutorialCanvas()
        {
            SettingsSave settings = SaveManager.LoadSettings();

            if (settings.tutorialShown) return;

            tutorialCanvas.SetActive(true);

            settings.SaveTutorialShown(true);

            SaveManager.SaveSettings(settings);
        }

        private void SwitchCanvas(GameObject canvasToSwitch)
        {
            if (_currentCanvas == canvasToSwitch)
            {
                if (!canvasToSwitch.activeSelf)
                {
                    canvasToSwitch.SetActive(true);

                    EventBus<Events.CanvasChanged>.Raise(new Events.CanvasChanged());
                }

                return;
            }

            if (_currentCanvas is not null) _previousCanvas = _currentCanvas;

            foreach (GameObject canvas in _canvases)
            {
                canvas.SetActive(false);
            }

            _currentCanvas = canvasToSwitch;

            canvasToSwitch.SetActive(true);

            EventBus<Events.CanvasChanged>.Raise(new Events.CanvasChanged());
        }

        public void SwitchCanvasWithDelay(GameObject canvasToSwitch, float delay)
        {
            StartCoroutine(DoSwitchCanvasWithDelay(canvasToSwitch, delay));
        }

        private IEnumerator DoSwitchCanvasWithDelay(GameObject canvasToSwitch, float delay)
        {
            yield return new WaitForSeconds(delay);
            SwitchCanvas(canvasToSwitch);
        }

        private void DisableAllCanvases()
        {
            foreach (GameObject canvas in _canvases)
            {
                canvas.SetActive(false);
            }
        }
    }
}