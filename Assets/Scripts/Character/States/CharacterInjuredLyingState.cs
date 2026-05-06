using Assets.Scripts.EventBus;
using UnityEngine;

namespace Assets.Scripts.Character.States
{
    public class CharacterInjuredLyingState : CharacterBaseState
    {
        private const float _injuredAnimationDelay = 1.5f;
        private float _injuryTick;
        private bool _isInjuryAnimationPlayed;
        private const float _deathStateDelay = 10f;
        private float _deathStateTick;
        private int _myCharacterId;

        public override void Enter()
        {
            base.Enter();
            AnimationsHandler.DeathAnimation();
            CharacterRagdollHandler.EnableInjuredRagdoll();
            CharacterMovementHandler.StopMovement();

            _myCharacterId = Context.gameObject.GetInstanceID();
        }

        public override void Update()
        {
            base.Update();

            _injuryTick += Time.deltaTime;

            if (_injuryTick >= _injuredAnimationDelay)
            {
                if (!_isInjuryAnimationPlayed)
                {
                    AnimationsHandler.InjuryAnimation();

                    _isInjuryAnimationPlayed = true;
                }
            }

            _deathStateTick += Time.deltaTime;

            if (_deathStateTick >= _deathStateDelay)
            {
                EventBus<Events.CharacterIsDeadAfterInjury>.Raise(new Events.CharacterIsDeadAfterInjury
                {
                    CharacterID = _myCharacterId
                });

                _deathStateTick = 0f;
            }
        }

        public override void Exit()
        {
            base.Exit();

            _deathStateTick = 0f;
        }
    }
}