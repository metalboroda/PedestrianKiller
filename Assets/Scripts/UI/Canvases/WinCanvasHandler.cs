using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.ScriptableObjects.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Canvases
{
    public class WinCanvasHandler : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private LevelDataSo levelData;
        [SerializeField] private CoinDataSo coinData;

        [Header("Coin Counter")]
        [SerializeField] private Text coinCounterText;
        
        [Header("References")]
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button restartButton;
        [Space]
        [SerializeField] private GameObject[] stars;

        private void OnEnable()
        {
            nextLevelButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.NextLevel,
                });
            });

            mainMenuButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.MainMenu,
                });
            });

            restartButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Restart
                });
            });

            StartCoroutine(DoSetRating(levelData.CurrentLevelRating));
        }

        private void OnDisable()
        {
            nextLevelButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            coinCounterText.text = coinData.CoinAmount.ToString();
        }

        private IEnumerator DoSetRating(int rating)
        {
            yield return null;

            if (stars.Length == 0) yield break;

            const int maxRating = 3;

            rating = Mathf.Clamp(rating, 0, maxRating);

            for (int i = 0; i < maxRating; i++)
            {
                stars[i].SetActive(i < rating);
            }
        }
    }
}