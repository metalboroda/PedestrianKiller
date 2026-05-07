namespace Assets.Scripts.Character.States
{
    public class CharacterCrouchAfraidState : CharacterBaseState
    {
        public override void Enter()
        {
            base.Enter();
            AnimationsHandler.CrouchAfraidAnimation();
            CharacterMovementHandler.StopMovement();
        }
    }
}