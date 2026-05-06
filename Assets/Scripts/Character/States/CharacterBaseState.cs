using Assets.Scripts.FSM;

namespace Assets.Scripts.Character.States
{
    public class CharacterBaseState : State<CharacterController>
    {
        protected FiniteStateMachine<CharacterController> StateMachine;
        protected StateFactory<CharacterController> StateFactory;

        protected CharacterAnimationsHandler AnimationsHandler;
        protected CharacterRagdollHandler CharacterRagdollHandler;
        protected CharacterMovementHandler CharacterMovementHandler;

        public override void Setup(CharacterController context)
        {
            base.Setup(context);

            StateMachine = context?.StateMachine;
            StateFactory = context?.StateFactory;

            AnimationsHandler = context?.GetComponent<CharacterAnimationsHandler>();
            CharacterRagdollHandler = context?.GetComponent<CharacterRagdollHandler>();
            CharacterMovementHandler = context?.GetComponent<CharacterMovementHandler>();
        }
    }
}