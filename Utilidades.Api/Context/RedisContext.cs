using StackExchange.Redis;

namespace Utilidades.Api.Context;

public class RedisContext(IConfiguration configuration) {
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection = new(() => ConnectionMultiplexer.Connect(
        configuration.GetConnectionString("REDIS") ?? "localhost:6379,allowAdmin=true,abortConnect=false"));

    public ConnectionMultiplexer Connection => _lazyConnection.Value;

    public IDatabase Database => Connection.GetDatabase();
};