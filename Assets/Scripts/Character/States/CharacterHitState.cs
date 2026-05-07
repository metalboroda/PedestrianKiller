using UnityEngine;

namespace Assets.Scripts.Character.States
{
    public class CharacterHitState : CharacterBaseState
    {
        private const float _hitDuration = 0.25f;
        private float _hitTick;
        private const int _runInjuredPercent = 8;
        private const int _crouchAfraidPercent = 5;

        public override void Enter()
        {
            base.Enter();
            CharacterMovementHandler.StopMovement();
            AnimationsHandler.HitAnimation();
        }

        public override void Update()
        {
            base.Update();

            _hitTick += Time.deltaTime;

            if (_hitTick >= _hitDuration)
            {
                int randomRunInjured = Random.Range(0, _runInjuredPercent);
                int randomCrouchAfraid = Random.Range(0, _crouchAfraidPercent);

                if (randomRunInjured == 0)
                    StateMachine.ChangeState(StateFactory.GetState<CharacterInjuredRunState>());
                else if (randomCrouchAfraid == 0)
                    StateMachine.ChangeState(StateFactory.GetState<CharacterCrouchAfraidState>());
                else
                    StateMachine.ChangeState(StateFactory.GetState<CharacterInjuredWalkState>());
            }
        }

        public override void Exit()
        {
            base.Exit();

            _hitTick = 0f;
        }
    }
}