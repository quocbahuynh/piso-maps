using System.Linq.Expressions;
using Hangfire;
using PISO.Contracts;

namespace PISO.HangfireService;

public class HangfireManager : IHangfireManager
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireManager(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    public string Enqueue(Expression<Action> methodCall) =>
        _backgroundJobClient.Enqueue(methodCall);

    public string Enqueue<T>(Expression<Action<T>> methodCall) =>
        _backgroundJobClient.Enqueue(methodCall);

    public string Schedule(Expression<Action> methodCall, TimeSpan delay) =>
        _backgroundJobClient.Schedule(methodCall, delay);

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay) =>
        _backgroundJobClient.Schedule(methodCall, delay);

    public void AddOrUpdate(string recurringJobId, Expression<Action> methodCall, string cronExpression) =>
        _recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression);

    public void AddOrUpdate<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression) =>
        _recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression);

    public bool RemoveIfExists(string recurringJobId)
    {
        try
        {
            _recurringJobManager.RemoveIfExists(recurringJobId);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
