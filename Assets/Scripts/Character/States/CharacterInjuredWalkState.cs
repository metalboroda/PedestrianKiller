namespace Assets.Scripts.Character.States
{
    public class CharacterInjuredWalkState : CharacterBaseState
    {
        public override void Enter()
        {
            base.Enter();
            AnimationsHandler.InjuredWalkAnimation();
        }

        public override void Update()
        {
            base.Update();
            CharacterMovementHandler.InjuredWalkForward();
        }
    }
}