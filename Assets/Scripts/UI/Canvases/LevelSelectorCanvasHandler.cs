using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.SaveSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Canvases
{
    public class LevelSelectorCanvasHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool unlockAllLevels;
        [Space]
        [SerializeField] private int itemsPerPage = 1;

        [Header("References")]
        [SerializeField] private GameObject container;
        [SerializeField] private Button backButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [Header("Configs")]
        [SerializeField] private LevelSelectorItemConfig[] levelSelectorItems;

        private readonly List<LevelSelectorItemHandler> _spawnedSelectorItemHandlers = new List<LevelSelectorItemHandler>();

        private int _totalPages;
        private int _currentPage;

        private LevelSave _levelSaveData;

        private void Awake()
        {
            _totalPages = Mathf.CeilToInt((float)levelSelectorItems.Length / itemsPerPage);
        }

        private void OnEnable()
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            nextButton.onClick.AddListener(OnNextButtonClicked);
            previousButton.onClick.AddListener(OnPreviousButtonClicked);

            LoadAndUpdateLevelUnlocks();

            _currentPage = unlockAllLevels ? 0 : GetPageOfLastUnlockedLevel();

            UpdatePaginationButtons();
            SpawnItems();
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
            previousButton.onClick.RemoveListener(OnPreviousButtonClicked);

            ClearSpawnedItems();
        }

        private int GetPageOfLastUnlockedLevel()
        {
            int lastUnlockedIndex = 0;

            for (int i = 0; i < levelSelectorItems.Length; i++)
            {
                if (levelSelectorItems[i].Unlocked)
                {
                    lastUnlockedIndex = i;
                }
            }

            return lastUnlockedIndex / itemsPerPage;
        }

        private void OnBackButtonClicked()
        {
            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
            {
                ButtonType = UiButtonType.Back,
            });
        }

        private void OnNextButtonClicked()
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;

                SpawnItems();
                UpdatePaginationButtons();

                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());
            }
        }

        private void OnPreviousButtonClicked()
        {
            if (_currentPage > 0)
            {
                _currentPage--;

                SpawnItems();
                UpdatePaginationButtons();

                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());
            }
        }

        private void UpdatePaginationButtons()
        {
            previousButton.interactable = _currentPage > 0;
            nextButton.interactable = _currentPage < _totalPages - 1;
        }

        private void ClearSpawnedItems()
        {
            foreach (LevelSelectorItemHandler handler in _spawnedSelectorItemHandlers)
            {
                Destroy(handler.gameObject);
            }

            _spawnedSelectorItemHandlers.Clear();
        }

        private void SpawnItems()
        {
            LoadAndUpdateLevelUnlocks();
            ClearSpawnedItems();

            int startIndex = _currentPage * itemsPerPage;
            int endIndex = Mathf.Min(startIndex + itemsPerPage, levelSelectorItems.Length);

            for (int i = startIndex; i < endIndex; i++)
            {
                LevelSelectorItemConfig itemConfig = levelSelectorItems[i];
                LevelSelectorItemHandler spawnedLevelSelectorItemHandler = Instantiate(itemConfig.LevelSelectorItemPrefab, container.transform)
                    .GetComponent<LevelSelectorItemHandler>();

                spawnedLevelSelectorItemHandler.SetName(itemConfig.Name);
                spawnedLevelSelectorItemHandler.SetIndex(i);
                spawnedLevelSelectorItemHandler.SetRating(itemConfig.Rating);
                spawnedLevelSelectorItemHandler.SetUnlocked(itemConfig.Unlocked);

                _spawnedSelectorItemHandlers.Add(spawnedLevelSelectorItemHandler);
            }
        }

        private void LoadAndUpdateLevelUnlocks()
        {
            _levelSaveData = SaveManager.LoadLevelSettings();

            for (int i = 0; i < levelSelectorItems.Length; i++)
            {
                int rating = _levelSaveData.GetLevelRating(i);
                levelSelectorItems[i].SetRating(rating);

                if (unlockAllLevels)
                {
                    levelSelectorItems[i].SetUnlocked(true);
                }
                else
                {
                    if (i == 0 || (i > 0 && _levelSaveData.GetLevelRating(i - 1) >= 3))
                    {
                        levelSelectorItems[i].SetUnlocked(true);
                    }
                }
            }
        }
    }
}