using UnityEngine;

namespace Assets.Scripts.Character.States
{
    public class CharacterInjuredLyingState : CharacterBaseState
    {
        private const float _injuredAnimationDelay = 1.5f;
        private float _injuryTick;
        private bool _isInjuryAnimationPlayed;

        public override void Enter()
        {
            base.Enter();
            AnimationsHandler.DeathAnimation();
            CharacterRagdollHandler.EnableInjuredRagdoll();
            CharacterMovementHandler.StopMovement();
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
        }
    }
}