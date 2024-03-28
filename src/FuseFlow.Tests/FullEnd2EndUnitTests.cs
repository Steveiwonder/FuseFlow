

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FuseFlow.Tests;

public class FullEnd2EndUnitTests
{
    [Fact]
    public async Task Test1Async()
    {
        IServiceCollection collection = new ServiceCollection();
        collection.AddSingleton<ILogger>(sp =>
        {
            return sp.GetRequiredService<ILogger<FullEnd2EndUnitTests>>();
        });
        collection.AddLogging(configure =>
        {
            configure.AddConsole();
        });
        var serviceProvider = collection.BuildServiceProvider();

        var job = new TestJob(serviceProvider);
        var beforeExecuteStates = new Dictionary<int, string>();
        var afterExecuteStates = new Dictionary<int, string>();
        for (int i = 0; i < 9; i++)
        {
            beforeExecuteStates.Add(i, job.CurrentState);
            await job.Execute(null);
            afterExecuteStates.Add(i, job.CurrentState);
        }

        Assert.Null(beforeExecuteStates[0]);
        Assert.Equal("pendingRestart", afterExecuteStates[0]);


        Assert.Equal("pendingRestart", beforeExecuteStates[1]);
        Assert.Equal("pendingRestart", afterExecuteStates[1]);


        Assert.Equal("pendingRestart", beforeExecuteStates[2]);
        Assert.Equal("restarting", afterExecuteStates[2]);


        Assert.Equal("restarting", beforeExecuteStates[3]);
        Assert.Equal("starting", afterExecuteStates[3]);


        Assert.Equal("starting", beforeExecuteStates[4]);
        Assert.Null(afterExecuteStates[4]);


    }
}

class TestJob : StateMachineJob
{
    class State1 : State.WithoutData
    {

        private int _executionCount = 0;
        public State1(ILogger logger)
        {
            _logger = logger;
        }

        private readonly ILogger _logger;


        public override string Name => "pendingRestart";


        public override  Task Execute(StateMachine stateMachine)
        {
            _executionCount++;
            if (_executionCount >= 3)
            {
                stateMachine.ChangeState<State2>();
                return Task.CompletedTask;
            }
            _logger.LogInformation("Pending restart....");
            return Task.CompletedTask;
        }
    }

    class State2 : State.WithoutData
    {
        public override string Name => "restarting";
        private ILogger _logger;
        public State2(ILogger logger)
        {
            _logger = logger;
        }


        public override Task Execute(StateMachine stateMachine)
        {
            _logger.LogInformation("Restarting...");

            stateMachine.ChangeState<State3>();

            return Task.CompletedTask;
        }
    }

    class State3 : State.WithoutData
    {
        public override string Name => "starting";
        private ILogger _logger;
        public State3(ILogger logger)
        {
            _logger = logger;
        }


        public override Task Execute(StateMachine stateMachine)
        {
            _logger.LogInformation("starting...");

            stateMachine.ChangeState(null);

            return Task.CompletedTask;
        }
    }
    public TestJob(IServiceProvider serviceProvider) : base(serviceProvider)
    {

    }

    protected override Type GetStateType(string state)
    {
        if (string.IsNullOrEmpty(state) || state.Equals(typeof(State1).Name))
        {
            return typeof(State1);
        }

        return null;
    }
}