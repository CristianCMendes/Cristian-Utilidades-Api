using StackExchange.Redis;

namespace Utilidades.Api.Context;

public class RedisContext {
    private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new(() => ConnectionMultiplexer.Connect("localhost:6379"));

    public static ConnectionMultiplexer Connection => LazyConnection.Value;

    public static IDatabase Database => Connection.GetDatabase();
};