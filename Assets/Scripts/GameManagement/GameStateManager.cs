using System;
using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using Assets.Scripts.FSM;
using Assets.Scripts.GameManagement.GameStates;
using Assets.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.Scripts.GameManagement
{
    public class GameStateManager : MonoBehaviour
    {
        public IFiniteStateMachine<GameStateManager> GameFsm { get; private set; }
        public StateFactory<GameStateManager> StateFactory { get; private set; }

        private EventBinding<Events.UIButtonClicked> _uiButtonClicked;
        private EventBinding<Events.SelectorItemPlayPressed> _selectorItemPlayPressed;

        private void Awake()
        {
            ServiceLocator.Register(this);

            GameFsm = new FiniteStateMachine<GameStateManager>(this);
            StateFactory = new StateFactory<GameStateManager>(this);

            GameFsm.StateChanged += OnGameStateChanged;
        }

        private void OnEnable()
        {
            _uiButtonClicked = new EventBinding<Events.UIButtonClicked>(OnUiButtonClicked);
            EventBus<Events.UIButtonClicked>.Register(_uiButtonClicked);
            _selectorItemPlayPressed = new EventBinding<Events.SelectorItemPlayPressed>(OnSelectorItemPlayPressed);
            EventBus<Events.SelectorItemPlayPressed>.Register(_selectorItemPlayPressed);
        }

        private void Start()
        {
            GameFsm.Initialize(StateFactory.GetState<GamePlayState>());
        }

        private void OnDisable()
        {
            EventBus<Events.UIButtonClicked>.Unregister(_uiButtonClicked);
            _uiButtonClicked = null;
            EventBus<Events.SelectorItemPlayPressed>.Unregister(_selectorItemPlayPressed);
            _selectorItemPlayPressed = null;
        }

        private void OnDestroy()
        {
            try
            {
                if (ServiceLocator.Get<GameStateManager>() == this)
                {
                    ServiceLocator.Unregister<GameStateManager>();
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"[GameStateManager] Could not unregister from ServiceLocator: {ex.Message}");
            }

            GameFsm.StateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(IState<GameStateManager> newState)
        {
            EventBus<Events.GameStateChanged>.Raise(new Events.GameStateChanged
            {
                State = newState
            });
        }

        private void OnUiButtonClicked(Events.UIButtonClicked uiButtonClicked)
        {
            switch (uiButtonClicked.ButtonType)
            {
                case UiButtonType.Default:
                    break;
                case UiButtonType.MainMenu:
                    GameFsm.ChangeState(StateFactory.GetState<GameMainMenuState>());
                    break;
                case UiButtonType.Pause:
                    GameFsm.ChangeState(StateFactory.GetState<GamePauseState>());
                    break;
                case UiButtonType.Restart:
                    GameFsm.ChangeState(StateFactory.GetState<GamePlayState>());
                    break;
                case UiButtonType.ResumeGame:
                    GameFsm.ChangeState(StateFactory.GetState<GamePlayState>());
                    break;
                case UiButtonType.NextLevel:
                    GameFsm.ChangeState(StateFactory.GetState<GameMainMenuState>());
                    break;
            }
        }

        private void OnSelectorItemPlayPressed(Events.SelectorItemPlayPressed selectorItemPlayPressed)
        {
            GameFsm.ChangeState(StateFactory.GetState<GamePlayState>());
        }
    }
}