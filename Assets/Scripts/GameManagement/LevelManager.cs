using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.GameManagement.GameStates;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.ScriptableObjects.Infrastructure;
using UnityEngine;

namespace Assets.Scripts.GameManagement
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private LevelDataSo levelDataSo;

        [Header("References")]
        [SerializeField] private GameObject[] levels;

        private GameObject _spawnedLevel;
        private int _currentLevelIndex;
        private int _currentLevelRating;

        private EventBinding<Events.SelectorItemPlayPressed> _selectorItemPlayPressed;
        private EventBinding<Events.GameStateChanged> _gameStateChanged;
        private EventBinding<Events.UIButtonClicked> _uiButtonClicked;

        private void OnEnable()
        {
            _selectorItemPlayPressed = new EventBinding<Events.SelectorItemPlayPressed>(OnSelectorItemPlayPressed);
            EventBus<Events.SelectorItemPlayPressed>.Register(_selectorItemPlayPressed);
            _gameStateChanged = new EventBinding<Events.GameStateChanged>(OnGameStateChanged);
            EventBus<Events.GameStateChanged>.Register(_gameStateChanged);
            _uiButtonClicked = new EventBinding<Events.UIButtonClicked>(OnUiButtonClicked);
            EventBus<Events.UIButtonClicked>.Register(_uiButtonClicked);
        }

        private void OnDisable()
        {
            EventBus<Events.SelectorItemPlayPressed>.Unregister(_selectorItemPlayPressed);
            EventBus<Events.GameStateChanged>.Unregister(_gameStateChanged);
            EventBus<Events.UIButtonClicked>.Unregister(_uiButtonClicked);
        }

        private void OnSelectorItemPlayPressed(Events.SelectorItemPlayPressed selectorItemPlayPressed)
        {
            _currentLevelIndex = selectorItemPlayPressed.Index;
            _currentLevelRating = selectorItemPlayPressed.Rating;

            SpawnLevel(_currentLevelIndex);
        }

        private void OnGameStateChanged(Events.GameStateChanged gameStateChanged)
        {
            if (gameStateChanged.State is not GameMainMenuState) return;

            if (_spawnedLevel is not null)
            {
                Destroy(_spawnedLevel);
            }

            _currentLevelIndex = 0;
            _currentLevelRating = 0;
        }

        private void OnUiButtonClicked(Events.UIButtonClicked uiButtonClicked)
        {
            if (uiButtonClicked.ButtonType != UiButtonType.Restart) return;

            Destroy(_spawnedLevel);
            SpawnLevel(_currentLevelIndex);
        }

        private void SpawnLevel(int index)
        {
            if (index < 0 || index >= levels.Length) return;

            _spawnedLevel = Instantiate(levels[index]);

            _currentLevelIndex = index;
        }

        private void CalculateLevelRating()
        {
            _currentLevelRating = 3;

            SaveLevelRating();
        }

        private void SaveLevelRating()
        {
            levelDataSo.CurrentLevelRating = _currentLevelRating;

            LevelSave levelSave = SaveManager.LoadLevelSettings();

            levelSave.SetLevelRating(_currentLevelIndex, _currentLevelRating);

            SaveManager.SaveLevelSettings(levelSave);
        }
    }
}