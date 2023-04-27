namespace AutoClash.Shared.Configs;

public class VpnProviderConfig
{
    /// <summary>
    /// 服务提供商名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 订阅地址
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 排除节点
    /// </summary>
    public string? ExcludeFilter { get; set; }

    /// <summary>
    /// 将订阅的节点按国家/地区, 生成分组(type:select)
    /// </summary>
    public bool GenCountryGroups { get; set; }
}