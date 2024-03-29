using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FuseFlow.Core;

public abstract class StateMachineJob
{
    private string _currentState;
    protected ILogger Logger;
    protected StateMachine stateMachine;
    public bool IsComplete { get; private set; }
    public string CurrentState => _currentState;
    public StateMachineJob(IServiceProvider serviceProvider)
    {
        Logger = serviceProvider.GetRequiredService<ILogger>();
        stateMachine = new StateMachine(Logger, serviceProvider);
    }


    public async Task Execute(string currentState)
    {
        if (IsComplete)
        {
            Logger.LogInformation("This job has complete");
            return;
        }

        //get the first state
        if (string.IsNullOrEmpty(_currentState))
        {
            Logger.LogInformation("Getting initial state");
            var initialStateType = GetStateType(currentState);
            if (initialStateType == null)
            {
                throw new Exception("Cannot find initial state");
            }
            // set the initial state
            Logger.LogInformation("Setting state machine initial state");
            stateMachine.ChangeState(initialStateType);



            _currentState = stateMachine.CurrentState.Name;
            Logger.LogInformation($"Initial state set {_currentState}");
        }

        Logger.LogInformation("Executing state machine");
        await stateMachine.Execute();
        Logger.LogInformation("Execution complete");
        if (stateMachine.CurrentState == null)
        {
            Logger.LogInformation("State machine has no state, job is complete");
            IsComplete = true;
            _currentState = null;
            return;
        }

        if (stateMachine.CurrentState is State.WithData)
        {
            var stateWithData = stateMachine.CurrentState as State.WithData;
            //persist data
        }
        currentState = stateMachine.CurrentState.Name;
        if (currentState != _currentState)
        {
            Logger.LogInformation($"State machine has transitioned from {_currentState} -> {currentState}");
            // the state has transitioned within the call to Execute();
            // store the new state so it can continue if something fails
            _currentState = stateMachine.CurrentState.Name;
        }

    }

    protected abstract Type GetStateType(string state);
}

