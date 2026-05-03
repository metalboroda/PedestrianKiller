using Assets.Scripts.EventBus;
using Assets.Scripts.ScriptableObjects.Infrastructure;
using Assets.Scripts.UI.Canvases;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ShopItemHandler : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CoinDataSo coinData;

        [Header("References")]
        [SerializeField] private Text priceText;
        [Space]
        [SerializeField] private Button buyButton;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button selectedButton;
        [Space]
        [SerializeField] private Image itemImage;
        [Space]
        [SerializeField] private GameObject priceObject;

        private EventBinding<Events.BuyResponse> _buyResponse;

        private string _itemName;
        private int _price;
        private bool _selected;
        private bool _unlocked;
        private ShopItemConfig _shopItemConfig;

        private ShopCanvasHandler _shopCanvasHandler;

        private void OnEnable()
        {
            _buyResponse = new EventBinding<Events.BuyResponse>(OnBuyResponse);
            EventBus<Events.BuyResponse>.Register(_buyResponse);

            buyButton.onClick.AddListener(BuyItem);
            selectButton.onClick.AddListener(SelectItem);

            UpdateBuyButtonState();
        }

        private void OnDisable()
        {
            EventBus<Events.BuyResponse>.Unregister(_buyResponse);

            buyButton.onClick.RemoveAllListeners();
            selectButton.onClick.RemoveAllListeners();
        }

        public ShopItemHandler SetName(string newName)
        {
            _itemName = newName;
            return this;
        }

        public ShopItemHandler SetPrice(int price)
        {
            _price = price;

            priceText.text = price.ToString();

            UpdateBuyButtonState();
            return this;
        }

        public void SetImage(Sprite sprite)
        {
            if (itemImage)
            {
                itemImage.sprite = sprite;
                itemImage.gameObject.SetActive(true);
            }
        }

        public void SetUnlocked(bool unlocked)
        {
            _unlocked = unlocked;

            priceText.gameObject.SetActive(!unlocked);
            buyButton.gameObject.SetActive(!unlocked);
            selectButton.gameObject.SetActive(unlocked);
            priceObject.SetActive(!unlocked);
            selectedButton.gameObject.SetActive(unlocked && _selected);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;

            selectedButton.gameObject.SetActive(_unlocked && selected);
        }

        public string GetName() => _itemName;

        public ShopItemHandler SetShopItem(ShopItemConfig shopItemConfig)
        {
            _shopItemConfig = shopItemConfig;
            return this;
        }

        public ShopItemHandler SetShopCanvasHandler(ShopCanvasHandler shopCanvasHandler)
        {
            _shopCanvasHandler = shopCanvasHandler;
            return this;
        }

        private void BuyItem()
        {
            EventBus<Events.BuyRequest>.Raise(new Events.BuyRequest
            {
                RequestName = _itemName,
                Price = _price
            });

            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());
        }

        private void UnlockItem()
        {
            _unlocked = true;

            priceObject.gameObject.SetActive(!_unlocked);
            priceText.gameObject.SetActive(!_unlocked);
            buyButton.gameObject.SetActive(!_unlocked);
            selectButton.gameObject.SetActive(_unlocked);
            _shopCanvasHandler.PurchaseItem(_shopItemConfig);
        }

        private void OnBuyResponse(Events.BuyResponse buyResponse)
        {
            if (buyResponse.ResponseName == _itemName && buyResponse.Response)
            {
                UnlockItem();
            }
        }

        private void SelectItem()
        {
            _shopCanvasHandler.SelectItem(this);

            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());
        }

        private void UpdateBuyButtonState()
        {
            if (buyButton is null || coinData is null) return;

            buyButton.interactable = coinData.CoinAmount >= _price;

            if (!_unlocked)
            {
                selectedButton.gameObject.SetActive(false);
            }
        }
    }
}