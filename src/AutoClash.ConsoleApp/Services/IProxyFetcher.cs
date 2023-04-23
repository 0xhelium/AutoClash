using AutoClash.Shared.Clash;

namespace AutoClash.Console.Services;

public interface IProxyFetcher
{
    public Task<List<Proxy>> GetProxies(string url);

}