using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.GameManagement.GameStates;
using Assets.Scripts.ScriptableObjects.Infrastructure;
using Lean.Pool;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    public class PoolContainer : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PoolContainerSo poolContainer;

        private EventBinding<Events.GameStateChanged> _gameStateChanged;
        private EventBinding<Events.UIButtonClicked> _uiButtonClicked;

        private void Awake()
        {
            poolContainer.PoolContainer = transform;
        }

        private void OnEnable()
        {
            _gameStateChanged = new EventBinding<Events.GameStateChanged>(OnGameStateChanged);
            EventBus<Events.GameStateChanged>.Register(_gameStateChanged);
            _uiButtonClicked = new EventBinding<Events.UIButtonClicked>(OnUIButtonClicked);
            EventBus<Events.UIButtonClicked>.Register(_uiButtonClicked);
        }

        private void OnDisable()
        {
            EventBus<Events.GameStateChanged>.Unregister(_gameStateChanged);
            _gameStateChanged = null;
            EventBus<Events.UIButtonClicked>.Unregister(_uiButtonClicked);
            _uiButtonClicked = null;
        }

        private void OnGameStateChanged(Events.GameStateChanged eventData)
        {
            if (eventData.State is GameMainMenuState)
            {
                DespawnAllChildren();
            }
        }

        private void OnUIButtonClicked(Events.UIButtonClicked eventData)
        {
            if (eventData.ButtonType is UiButtonType.Restart)
            {
                DespawnAllChildren();
            }
        }


        private void DespawnAllChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                LeanPool.Despawn(transform.GetChild(i).gameObject);
            }
        }
    }
}