namespace Assets.Scripts.Character.States
{
    public class CharacterInjuredRunState : CharacterBaseState
    {
        public override void Enter()
        {
            base.Enter();
            AnimationsHandler.InjuredRunAnimation();
        }

        public override void Update()
        {
            base.Update();
            CharacterMovementHandler.InjuredRunForward();
        }
    }
}