namespace Assets.Scripts.Character.States
{
    public class CharacterWalkState : CharacterBaseState
    {
        public override void Enter()
        {
            base.Enter();
            AnimationsHandler.WalkAnimation();
        }

        public override void Update()
        {
            base.Update();
            CharacterMovementHandler.WalkForward();
        }
    }
}