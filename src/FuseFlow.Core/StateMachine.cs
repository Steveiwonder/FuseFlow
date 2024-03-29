

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FuseFlow.Core;

public class StateMachine
{
    private IState _currentState;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public IState CurrentState => _currentState;

    public StateMachine(ILogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }


    private IState CreateState(Type type)
    {
        return ActivatorUtilities.CreateInstance(_serviceProvider, type) as IState;
    }

    public void ChangeState<TState>() where TState : class, IState
    {
        ChangeState(typeof(TState));
    }

    public void ChangeState(Type type)
    {
        if (type == null)
        {
            End();
            return;
        }
        IState newState = CreateState(type);
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }

    public void End()
    {
        if (_currentState == null)
        {
            return;
        }
        _currentState.Exit(this);
        _currentState = null;
    }

    public Task Execute()
    {
        return _currentState?.Execute(this);
    }

    public void Log(string message)
    {
        _logger.LogInformation(message);
    }
}

