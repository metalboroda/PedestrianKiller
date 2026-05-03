using Assets.Scripts.Character.States;
using Assets.Scripts.EventBus;
using Assets.Scripts.FSM;
using UnityEngine;

namespace Assets.Scripts.Character
{
    [RequireComponent(typeof(CharacterHealthHandler))]
    [RequireComponent(typeof(CharacterAnimationsHandler))]
    [RequireComponent(typeof(CharacterRagdollHandler))]
    [RequireComponent(typeof(CharacterMovementHandler))]
    [RequireComponent(typeof(CharacterDecalVfxHandler))]
    public class CharacterController : MonoBehaviour
    {
        public FiniteStateMachine<CharacterController> StateMachine { get; private set; }
        public StateFactory<CharacterController> StateFactory { get; private set; }

        private EventBinding<Events.BodyPartDamaged> _bodyPartDamagedBinding;
        private EventBinding<Events.CharacterIsInjured> _characterIsInjuredBinding;
        private EventBinding<Events.CharacterIsDead> _characterIsDeadBinding;

        private bool _isInjured;
        private bool _isDead;

        private void Awake()
        {
            StateMachine = new FiniteStateMachine<CharacterController>(this);
            StateFactory = new StateFactory<CharacterController>(this);
        }

        private void OnEnable()
        {
            _bodyPartDamagedBinding = new EventBinding<Events.BodyPartDamaged>(OnBodyPartDamaged);
            EventBus<Events.BodyPartDamaged>.Register(_bodyPartDamagedBinding);
            _characterIsInjuredBinding = new EventBinding<Events.CharacterIsInjured>(OnCharacterIsInjured);
            EventBus<Events.CharacterIsInjured>.Register(_characterIsInjuredBinding);
            _characterIsDeadBinding = new EventBinding<Events.CharacterIsDead>(OnCharacterIsDead);
            EventBus<Events.CharacterIsDead>.Register(_characterIsDeadBinding);
        }

        private void Start()
        {
            StateMachine.Initialize(StateFactory.GetState<CharacterWalkState>());
        }

        private void Update()
        {
            StateMachine.CurrentState.Update();
        }

        private void OnDisable()
        {
            EventBus<Events.BodyPartDamaged>.Unregister(_bodyPartDamagedBinding);
            EventBus<Events.CharacterIsInjured>.Unregister(_characterIsInjuredBinding);
            EventBus<Events.CharacterIsDead>.Unregister(_characterIsDeadBinding);
        }

        private void OnBodyPartDamaged(Events.BodyPartDamaged eventData)
        {
            if (eventData.CharacterID != gameObject.GetInstanceID()) return;
            if (_isDead) return;
            if (_isInjured) return;
            if (StateMachine.CurrentState is not CharacterHitState)
            {
                StateMachine.ChangeState(StateFactory.GetState<CharacterHitState>());
            }
        }

        private void OnCharacterIsInjured(Events.CharacterIsInjured eventData)
        {
            if (eventData.CharacterID != gameObject.GetInstanceID()) return;
            if (_isDead) return;
            if (_isInjured) return;

            _isInjured = true;

            StateMachine.ChangeState(StateFactory.GetState<CharacterInjuredLyingState>());
        }

        private void OnCharacterIsDead(Events.CharacterIsDead eventData)
        {
            if (eventData.CharacterID == gameObject.GetInstanceID())
            {
                _isDead = true;

                StateMachine.ChangeState(StateFactory.GetState<CharacterDeathState>());
            }
        }
    }
}