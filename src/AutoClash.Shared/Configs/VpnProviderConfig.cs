namespace AutoClash.Shared.Configs;

public class VpnProviderConfig
{
    public VpnProviderConfig(string name, string url)
    {
        Name = name;
        Url = url;
    }

    /// <summary>
    /// 服务提供商名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 订阅地址
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    /// 排除节点
    /// </summary>
    public string? ExcludeFilter { get; set; }
    
}