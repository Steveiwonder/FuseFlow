

using System.Reflection;
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
    private Func<StateMachineJob> _getJob;

    public StateMachine(ILogger logger, IServiceProvider serviceProvider, Func<string> getStateData, Action<string> setStateData, Func<StateMachineJob> getJob)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _getStateData = getStateData;
        _setStateData = setStateData;
        _getJob = getJob;
    }


    private IState CreateState(Type type)
    {
        return ActivatorUtilities.CreateInstance(_serviceProvider, type) as IState;
    }

    public void ChangeState<TState>() where TState : class, IState
    {
        ChangeState(typeof(TState));
    }

    private bool IsAssignableToGenericType(object obj, Type genericType)
    {
        Type type = obj.GetType();
        while (type != null && type != typeof(object))
        {
            var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (genericType == cur)
            {
                return true;
            }
            type = type.BaseType;
        }

        foreach (var i in obj.GetType().GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
        }

        return false;
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
        if (IsAssignableToGenericType(newState, typeof(State<>)))
        {
            MethodInfo method = newState.GetType().GetMethod("SetJob");
            method?.Invoke(newState, new object[] { _getJob() });
        }
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

