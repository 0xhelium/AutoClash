using AutoClash.Shared.Clash;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AutoClash.Console.Services;

public class ProxyFetcher : IProxyFetcher
{
    private readonly IHttpClientFactory _factory;

    public ProxyFetcher(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<Proxy>> GetProxies(string url)
    {
        var result = new List<Proxy>();
        var client = _factory.CreateClient("default");
        var response = await client.GetStringAsync(url);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var config = deserializer.Deserialize<Dictionary<string, object>>(response);
        var ps = config.TryGetValue("proxies", out var conf)
            ? conf as List<object>
            : default;
        if (ps is null)
        {
            return result;
        }
        
        foreach (var item in ps )
        {
            if (item is  Dictionary<object,object> dict)
            {
                var dic = new Proxy(dict.ToDictionary(x => x.Key.ToString()!, x => x.Value));
                result.Add(dic);
            }
        }

        return result;
    }
}