namespace Miyo.Core.StateMachine
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public void TransitionTo(IState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }
}
