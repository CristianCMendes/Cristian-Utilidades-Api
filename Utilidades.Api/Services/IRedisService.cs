namespace Utilidades.Api.Services;

public interface IRedisService {
    public T? Get<T>(string key);
    public void Set<T>(string key, T? value);
    public void Remove(string key);
}