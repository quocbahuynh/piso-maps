using System.Text.Json;
using PISO.Contracts;
using StackExchange.Redis;

namespace PISO.RedisService;

public class RedisManager : IRedisManager
{
    private readonly IDatabase _database;

    public RedisManager(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        if (expiration.HasValue)
        {
            await _database.StringSetAsync(key, json, expiration.Value);
        }
        else
        {
            await _database.StringSetAsync(key, json);
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<long> IncrementAsync(string key, TimeSpan? expiration = null)
    {
        var result = await _database.StringIncrementAsync(key);
        if (result == 1 && expiration.HasValue)
        {
            await _database.KeyExpireAsync(key, expiration.Value);
        }

        return result;
    }

    public async Task PushToFixedListAsync<T>(string key, T value, int maxListSize)
    {
        var serializedValue = JsonSerializer.Serialize(value);

        await _database.ListLeftPushAsync(key, serializedValue);
        await _database.ListTrimAsync(key, 0, maxListSize - 1);
    }

    public async Task<System.Collections.Generic.IEnumerable<T>> GetListAsync<T>(string key)
    {
        var values = await _database.ListRangeAsync(key, 0, -1);

        var list = new System.Collections.Generic.List<T>();
        foreach (var val in values)
        {
            if (val.HasValue)
            {
                var item = JsonSerializer.Deserialize<T>(val!);
                if (item is not null)
                {
                    list.Add(item);
                }
            }
        }

        return list;
    }

    public async Task AddToSetAsync(string key, string value, TimeSpan? expiration = null)
    {
        await _database.SetAddAsync(key, value);
        if (expiration.HasValue)
        {
            await _database.KeyExpireAsync(key, expiration.Value);
        }
    }

    public async Task<IEnumerable<string>> GetSetMembersAsync(string key)
    {
        var members = await _database.SetMembersAsync(key);
        var result = new System.Collections.Generic.List<string>();
        foreach (var member in members)
        {
            if (member.HasValue) result.Add(member.ToString());
        }
        return result;
    }

    public async Task<long> GetLongAsync(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (!value.HasValue) return 0;

        return long.TryParse(value.ToString(), out var result) ? result : 0;
    }
}
