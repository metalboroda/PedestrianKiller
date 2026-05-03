using Assets.Scripts.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class LevelSelectorItemHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text nameText;
        [Space]
        [SerializeField] private Button playButton;
        [Space]
        [SerializeField] private GameObject frameOn;
        [SerializeField] private GameObject frameOff;
        [Space]
        [SerializeField] private GameObject[] ratingStars;

        private int _index;
        private bool _isFirstItem;
        private bool _buttonPressed;

        private void OnEnable()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);

            _buttonPressed = false;
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }

        private void OnPlayButtonClicked()
        {
            if (_buttonPressed) return;

            EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked());
            EventBus<Events.SelectorItemPlayPressed>.Raise(new Events.SelectorItemPlayPressed
            {
                Index = _index,
            });

            _buttonPressed = true;
        }

        public void SetName(string newName)
        {
            if (nameText)
            {
                nameText.text = newName;
            }
        }

        public void SetIndex(int index)
        {
            _index = index;

            _isFirstItem = index == 0;
        }

        public void SetRating(int rating)
        {
            const int maxRating = 3;
            int clampedRating = Mathf.Clamp(rating, 0, maxRating);

            for (int i = 0; i < ratingStars.Length; i++)
            {
                if (ratingStars[i] is not null)
                {
                    bool isActive = i < clampedRating;

                    ratingStars[i].SetActive(isActive);
                }
            }
        }

        public void SetUnlocked(bool unlocked)
        {
            if (_isFirstItem)
            {
                unlocked = true;
            }

            playButton.gameObject.SetActive(unlocked);

            frameOn.SetActive(unlocked);
            frameOff.SetActive(!unlocked);
        }
    }
}