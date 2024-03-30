

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FuseFlow.Core;
using System.Runtime.CompilerServices;
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

        collection.AddSingleton<IJobDispatcher, JobDispatcher>();
        collection.AddSingleton<IJobOrchestrator, JobOrchestrator>();
        collection.AddSingleton<IJobStore, InMemoryJobStore>();
        collection.AddSingleton<IDataSerializer, JsonJobParameterSerializer>();
        collection.AddSingleton<IJobStateStore>((sp) => sp.GetRequiredService<IJobStore>() as InMemoryJobStore);
        collection.AddLogging(configure =>
        {
            configure.AddConsole();
        });
        var serviceProvider = collection.BuildServiceProvider();

        var jobOrchestrator = serviceProvider.GetRequiredService<IJobOrchestrator>();
        var jobDispatcher = serviceProvider.GetRequiredService<IJobDispatcher>();
        var jobStore = serviceProvider.GetRequiredService<IJobStore>();
        await jobDispatcher.Dispatch<TestJob>("mytestwebapp");


        var states = new List<string>();
        var stateData = new Dictionary<int, string>();
        CancellationToken cancellationToken = CancellationToken.None;
        var job = jobStore.GetAllJobs().First();
        for (int i = 0; i < 5; i++)
        {
            states.Add(job.CurrentState);
            await jobOrchestrator.ExecuteAsync(cancellationToken);
            stateData[i] = job.CurrentStateData;
            states.Add(job.CurrentState);
        }

        Assert.Null(states[0]);
        Assert.Equal("pendingRestart", states[1]);


        Assert.Equal("pendingRestart", states[2]);
        Assert.Equal("pendingRestart", states[3]);


        Assert.Equal("pendingRestart", states[4]);
        Assert.Equal("restarting", states[5]);


        Assert.Equal("restarting", states[6]);
        Assert.Equal("starting", states[7]);


        Assert.Equal("starting", states[8]);
        Assert.Null(states[9]);


    }
}

class TestJob : StateMachineJob
{
    class State1 : State
    {

        private int _executionCount = 0;
        public State1(ILogger logger, IDataSerializer dataSerializer)
        {
            _logger = logger;
            _dataSerializer = dataSerializer;
        }

        private readonly ILogger _logger;
        private readonly IDataSerializer _dataSerializer;

        public override string Name => "pendingRestart";


        public override async void Enter(StateMachine stateMachine)
        {
            var data = stateMachine.GetStateData();
            if (!string.IsNullOrEmpty(data))
            {
                _executionCount = await _dataSerializer.Deserialize<int>(stateMachine.GetStateData());
            }
        }

        public override async Task Execute(StateMachine stateMachine)
        {
            _executionCount++;
            if (_executionCount >= 3)
            {
                stateMachine.ChangeState<State2>();
                return;
            }
            _logger.LogInformation("Pending restart....");
            stateMachine.SetStateData(this, await _dataSerializer.Serialize(_executionCount));
        }
    }

    class State2 : State
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

    class State3 : State
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

            stateMachine.End();

            return Task.CompletedTask;
        }
    }

    private IDataSerializer _jobParamSerializer;
    private string _webAppName;
    public TestJob(IServiceProvider serviceProvider, IDataSerializer jobParamSerializer) : base(serviceProvider)
    {
        _jobParamSerializer = jobParamSerializer;
    }

    protected override Type GetStateType(string state)
    {
        if (string.IsNullOrEmpty(state) || state.Equals(typeof(State1).Name))
        {
            return typeof(State1);
        }

        return null;
    }

    public override async Task Configure(IJobDetail jobDetail)
    {
        _webAppName = await _jobParamSerializer.Deserialize<string>(jobDetail.Parameters);
        await base.Configure(jobDetail);
    }
}