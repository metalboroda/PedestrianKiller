namespace Assets.Scripts.FSM
{
    public interface IState<in TContext> where TContext : class
    {
        public void Setup(TContext context);
        
        public void Enter();
        
        public void Exit();
        
        public void Update();
        
        public void FixedUpdate();
        
        public void LateUpdate();
    }
}