using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.ScriptableObjects.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Canvases
{
    public class ShopCanvasHandler : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CoinDataSo coinData;

        [Header("Coin Counter")]
        [SerializeField] private Text coinCounterText;

        [Header("References")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [Space]
        [SerializeField] private Image coinCounterImage;
        [Space]
        [SerializeField] private Transform prefabSpawnPoint;
        [SerializeField] private Transform container;

        [Header("Configs")]
        [SerializeField] private ShopItemConfig[] shopItems;

        private const int ItemsPerPage = 1;

        private readonly List<ShopItemHandler> _spawnedItems = new List<ShopItemHandler>();
        private int _totalPages;
        private int _currentPage;
        private string _selectedItemName;
        private GameObject _spawnedPrefab;
        private ShopSave _shopSave;


        private void Awake()
        {
            _totalPages = Mathf.CeilToInt((float)shopItems.Length / ItemsPerPage);
        }

        private void OnEnable()
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            nextButton.onClick.AddListener(OnNextButtonClicked);
            previousButton.onClick.AddListener(OnPreviousButtonClicked);

            LoadShopSave();

            _currentPage = GetPageForSelectedItem();

            UpdatePaginationButtons();
            SpawnShopItems();
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            previousButton.onClick.RemoveAllListeners();

            ClearSpawnedItems();
            DestroySpawnedPrefab();
        }

        private void Update()
        {
            coinCounterText.text = coinData.CoinAmount.ToString();
        }

        private void OnBackButtonClicked()
        {
            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
            {
                ButtonType = UiButtonType.Back
            });

            SaveManager.SaveShopSettings(_shopSave);
        }

        private void OnNextButtonClicked()
        {
            if (_currentPage >= _totalPages - 1) return;

            _currentPage++;

            SpawnShopItems();
            UpdatePaginationButtons();

            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());
        }

        private void OnPreviousButtonClicked()
        {
            if (_currentPage <= 0) return;

            _currentPage--;

            SpawnShopItems();
            UpdatePaginationButtons();

            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());
        }

        private int GetPageForSelectedItem()
        {
            if (string.IsNullOrEmpty(_selectedItemName)) return 0;

            for (int i = 0; i < shopItems.Length; i++)
            {
                if (shopItems[i].Name == _selectedItemName)
                {
                    return i / ItemsPerPage;
                }
            }

            return 0;
        }

        private void UpdatePaginationButtons()
        {
            previousButton.interactable = _currentPage > 0;

            nextButton.interactable = _currentPage < _totalPages - 1;
        }

        private void ClearSpawnedItems()
        {
            foreach (ShopItemHandler handler in _spawnedItems)
            {
                Destroy(handler.gameObject);
            }

            _spawnedItems.Clear();
        }

        private void DestroySpawnedPrefab()
        {
            if (_spawnedPrefab is null) return;

            Destroy(_spawnedPrefab);

            _spawnedPrefab = null;
        }

        private void SpawnShopItems()
        {
            ClearSpawnedItems();
            DestroySpawnedPrefab();

            int startIndex = _currentPage * ItemsPerPage;
            int endIndex = Mathf.Min(startIndex + ItemsPerPage, shopItems.Length);

            for (int i = startIndex; i < endIndex; i++)
            {
                ShopItemConfig itemConfig = shopItems[i];
                bool isUnlocked = i == 0 || _shopSave.unlockedItems.Contains(itemConfig.Name);
                ShopItemHandler shopItemHandler = Instantiate(itemConfig.ShopItemHandler, container)
                    .SetName(itemConfig.Name)
                    .SetPrice(itemConfig.Price)
                    .SetShopItem(itemConfig)
                    .SetShopCanvasHandler(this);

                shopItemHandler.SetUnlocked(isUnlocked);
                shopItemHandler.SetSelected(itemConfig.Name == _selectedItemName);

                _spawnedItems.Add(shopItemHandler);

                if (itemConfig.UseImageInsteadOfPrefab)
                {
                    shopItemHandler.SetImage(itemConfig.ItemImage);
                }
                else if (itemConfig.Prefab is not null)
                {
                    if (prefabSpawnPoint)
                    {
                        _spawnedPrefab = Instantiate(itemConfig.Prefab, prefabSpawnPoint.position, Quaternion.identity);
                        _spawnedPrefab.transform.localScale = Vector3.zero;
                    }
                }
            }
        }

        private void LoadShopSave()
        {
            _shopSave = SaveManager.LoadShopSettings();

            _selectedItemName = _shopSave.selectedItemName;

            if (!string.IsNullOrEmpty(_selectedItemName) && _shopSave.unlockedItems.Contains(_selectedItemName)) return;

            _selectedItemName = shopItems[0].Name;

            _shopSave.selectedItemName = _selectedItemName;

            SaveManager.SaveShopSettings(_shopSave);
        }

        public void PurchaseItem(ShopItemConfig itemConfig)
        {
            itemConfig.SetUnlocked(true);

            if (_shopSave.unlockedItems.Contains(itemConfig.Name) || itemConfig.Name == shopItems[0].Name) return;

            _shopSave.unlockedItems.Add(itemConfig.Name);

            SaveManager.SaveShopSettings(_shopSave);
        }

        public void SelectItem(ShopItemHandler itemHandler)
        {
            foreach (ShopItemHandler shopItemHandler in _spawnedItems)
            {
                shopItemHandler.SetSelected(false);
            }

            itemHandler.SetSelected(true);

            _selectedItemName = itemHandler.GetName();

            _shopSave.selectedItemName = _selectedItemName;

            SaveManager.SaveShopSettings(_shopSave);
        }
    }
}