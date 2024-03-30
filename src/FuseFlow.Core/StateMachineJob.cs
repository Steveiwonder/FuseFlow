using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FuseFlow.Core;
public abstract class StateMachineJob : IJob
{
    protected ILogger Logger;
    protected StateMachine stateMachine;
    public bool IsComplete { get; private set; }
    public string CurrentState { get; private set; }
    protected IJobDetail JobDetail { get; private set; }

    private readonly IJobStateStore _jobStateStore;

    public StateMachineJob(IServiceProvider serviceProvider)
    {
        _jobStateStore = serviceProvider.GetRequiredService<IJobStateStore>();
        Logger = serviceProvider.GetRequiredService<ILogger>();
        stateMachine = new StateMachine(Logger, serviceProvider, () =>
        {
            return _jobStateStore.GetStateData(JobDetail.JobId);
        }, (data) =>
        {
            _jobStateStore.SetStateData(JobDetail.JobId, data);
        });
    }


    public async Task Execute(CancellationToken stoppingToken)
    {
        if (IsComplete)
        {
            Logger.LogInformation("This job has complete");
            return;
        }



        Logger.LogInformation("Executing state machine");
        await stateMachine.Execute();
        Logger.LogInformation("Execution complete");
        if (stateMachine.CurrentState == null)
        {
            Logger.LogInformation("State machine has no state, job is complete");
            IsComplete = true;
            CurrentState = null;
            return;
        }


        var currentState = stateMachine.CurrentState.Name;
        if (currentState != CurrentState)
        {
            Logger.LogInformation($"State machine has transitioned from {CurrentState} -> {currentState}");
            // the state has transitioned within the call to Execute();
            // store the new state so it can continue if something fails
            CurrentState = stateMachine.CurrentState.Name;
        }

    }

    protected abstract Type GetStateType(string state);

    public virtual Task Configure(IJobDetail jobDetail)
    {
        JobDetail = jobDetail;
        Logger.LogInformation("Getting initial state");
        var initialStateType = GetStateType(jobDetail.CurrentState);
        if (initialStateType == null)
        {
            throw new Exception("Cannot find initial state");
        }
        // set the initial state
        Logger.LogInformation("Setting state machine initial state");
        stateMachine.ChangeState(initialStateType);
        CurrentState = stateMachine.CurrentState.Name;
        Logger.LogInformation($"Initial state set {CurrentState}");

        return Task.CompletedTask;
    }
}

