using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.ScriptableObjects.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Canvases
{
    public class GameCanvasHandler : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CoinDataSo coinData;

        [Header("Damage Vignette Settings")]
        [SerializeField] private float damageDuration;
        [Space]
        [SerializeField] private Color normalVignetteColor;
        [SerializeField] private Color damagedVignetteColor;
        [Space]
        [SerializeField] private Image damageVignette;
        
        [Header("Coin Counter")]
        [SerializeField] private Text coinCounterText;
        [Space]
        [SerializeField] private Image coinCounterIcon;

        [Header("References")]
        [SerializeField] private Button pauseButton;

        private void OnEnable()
        {
            pauseButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Pause
                });
            });

            coinData.CoinAmountChanged += OnCoinAmountChanged;

            coinCounterText.text = coinData.CoinAmount.ToString();
        }

        private void OnDisable()
        {
            pauseButton.onClick.RemoveAllListeners();

            coinData.CoinAmountChanged -= OnCoinAmountChanged;
        }

        private void OnCoinAmountChanged(int coinAmount)
        {
            coinCounterText.text = coinAmount.ToString();
        }
    }
}