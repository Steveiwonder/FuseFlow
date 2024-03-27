

namespace StateMachineDemo
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
        public abstract class WithData<T> : IState
        {
            public abstract string Name { get; }

            public void Enter(StateMachine stateMachine) { }
            public abstract Task Execute(StateMachine stateMachine);
            public void Exit(StateMachine stateMachine) { }
            public abstract T GetData();
            public abstract void SetData(T data);
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