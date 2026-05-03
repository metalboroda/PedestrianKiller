namespace Assets.Scripts.Character.States
{
    public class CharacterDeathState : CharacterBaseState
    {
        public override void Enter()
        {
            base.Enter();

            if (StateMachine.PreviousState != StateFactory.GetState<CharacterInjuredLyingState>())
                AnimationsHandler.DeathAnimation();
            else
                AnimationsHandler.StopAnimator();

            CharacterRagdollHandler.EnableDeadRagdoll();
            CharacterMovementHandler.StopMovement();
        }
    }
}