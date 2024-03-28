

using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FuseFlow
{
    class RestartWebAppJob : StateMachineJob
    {
        class PendingRestartState : State.WithoutData
        {

            private int _executionCount = 0;
            public PendingRestartState(ILogger logger)
            {
                _logger = logger;
            }

            private readonly ILogger _logger;


            public override string Name => "pendingRestart";


            public override async Task Execute(StateMachine stateMachine)
            {
                _executionCount++;
                if (_executionCount >= 3)
                {
                    stateMachine.ChangeState<RestartingState>();
                    return;
                }
                _logger.LogInformation("Pending restart....");
                await Task.Delay(3000);

            }


        }

        class RestartingState : State.WithoutData
        {
            public override string Name => "restarting";
            private ILogger _logger;
            public RestartingState(ILogger logger)
            {
                _logger = logger;
            }


            public override Task Execute(StateMachine stateMachine)
            {
                _logger.LogInformation("Restarting...");

                stateMachine.ChangeState<StartingState>();

                return Task.CompletedTask;
            }


        }

        class StartingState : State.WithoutData
        {
            public override string Name => "starting";
            private ILogger _logger;
            public StartingState(ILogger logger)
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
        public RestartWebAppJob(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Type GetStateType(string state)
        {
            if (string.IsNullOrEmpty(state) || state.Equals(typeof(PendingRestartState).Name))
            {
                return typeof(PendingRestartState);
            }

            return null;
        }
    }

}