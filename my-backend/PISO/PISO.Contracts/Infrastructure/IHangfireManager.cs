using System.Linq.Expressions;

namespace PISO.Contracts;

public interface IHangfireManager
{
    string Enqueue(Expression<Action> methodCall);
    string Enqueue<T>(Expression<Action<T>> methodCall);

    string Schedule(Expression<Action> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);

    void AddOrUpdate(string recurringJobId, Expression<Action> methodCall, string cronExpression);
    void AddOrUpdate<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression);

    bool RemoveIfExists(string recurringJobId);
}
