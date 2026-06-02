namespace PISO.Contracts;

public interface IRedisManager
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<T?> GetAsync<T>(string key);
    Task<bool> RemoveAsync(string key);
    Task<long> IncrementAsync(string key, TimeSpan? expiration = null);
    Task PushToFixedListAsync<T>(string key, T value, int maxListSize);
    Task<IEnumerable<T>> GetListAsync<T>(string key);
    Task AddToSetAsync(string key, string value, TimeSpan? expiration = null);
    Task<IEnumerable<string>> GetSetMembersAsync(string key);
    Task<long> GetLongAsync(string key);
}
