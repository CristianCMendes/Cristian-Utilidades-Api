using System.Text.Json;
using StackExchange.Redis;
using Utilidades.Api.Context;

namespace Utilidades.Api.Services;

public class RedisService(RedisContext redisContext) : IRedisService {
    private readonly IDatabase _database = redisContext.Database;

    /// <inheritdoc />
    public T? Get<T>(string key) {
        var value = _database.StringGet(key);

        if (!value.HasValue || string.IsNullOrEmpty(value)) return default;

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    /// <inheritdoc />
    public void Set<T>(string key, T? value, TimeSpan? expiry = null) {
        if (value == null) return;
        _database.StringSet(key, JsonSerializer.Serialize(value), expiry);
    }

    /// <inheritdoc />
    public void Remove(string key) {
        _database.KeyDelete(key);
    }
};