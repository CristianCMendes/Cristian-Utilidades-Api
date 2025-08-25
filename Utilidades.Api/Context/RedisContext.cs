using StackExchange.Redis;

namespace Utilidades.Api.Context;

public class RedisContext {
    private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new(() => ConnectionMultiplexer.Connect(
        Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_REDIS") ?? "localhost:6379,allowAdmin=true,abortConnect=false"));

    public static ConnectionMultiplexer Connection => LazyConnection.Value;

    public static IDatabase Database => Connection.GetDatabase();
};