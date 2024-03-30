

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FuseFlow.Core;

public class StateMachine
{
    private IState _currentState;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public IState CurrentState => _currentState;
    private Action<string> _setStateData;
    private Func<string> _getStateData;

    public StateMachine(ILogger logger, IServiceProvider serviceProvider, Func<string> getStateData, Action<string> setStateData)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _getStateData = getStateData;
        _setStateData = setStateData;
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

        ClearStateData();
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

    public void SetStateData(IState sender, string data)
    {
        if (sender != CurrentState)
        {
            throw new Exception("You cannot set state data unless you are the current state, transition first");
        }
        _setStateData(data);
    }

    public string GetStateData()
    {
        return _getStateData();
    }

    public void ClearStateData()
    {
        SetStateData(CurrentState, null);
    }
}

