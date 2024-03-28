

namespace FuseFlow
{
    public interface IState
    {
        string Name { get; }
        void Enter(StateMachine stateMachine);
        Task Execute(StateMachine stateMachine);
        void Exit(StateMachine stateMachine);

    }

    public class State
    {
        public abstract class WithData : IState
        {
            public abstract string Name { get; }

            public void Enter(StateMachine stateMachine) { }
            public abstract Task Execute(StateMachine stateMachine);
            public void Exit(StateMachine stateMachine) { }
            public abstract string GetData();
            public abstract void SetData(string data);
        }

        public abstract class WithoutData : IState
        {
            public abstract string Name { get; }

            public void Enter(StateMachine stateMachine) { }
            public abstract Task Execute(StateMachine stateMachine);
            public void Exit(StateMachine stateMachine) { }
        }
    }

}