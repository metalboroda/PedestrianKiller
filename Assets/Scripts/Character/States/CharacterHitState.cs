using UnityEngine;

namespace Assets.Scripts.Character.States
{
    public class CharacterHitState : CharacterBaseState
    {
        private const float _hitDuration = 0.25f;
        private float _hitTick;
        private const int _runPercent = 8;

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
                int randomRun = Random.Range(0, _runPercent);

                if (randomRun == 0)
                    StateMachine.ChangeState(StateFactory.GetState<CharacterInjuredRunState>());
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