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
        //var response = await client.GetStringAsync(url);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
        if (!response.IsSuccessStatusCode)
        {
            return new List<Proxy>();
        }
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var config = deserializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
        var ps = config.TryGetValue("proxies", out var conf)
            ? conf as List<object>
            : default;
        if (ps is null)
        {
            return result;
        }
        
        foreach (var item in ps )
        {
            if (item is not Dictionary<object, object> dict) continue;
            
            var dic = new Proxy(dict.ToDictionary(x => x.Key.ToString()!, x => x.Value));
            result.Add(dic);
        }

        return result;
    }
}