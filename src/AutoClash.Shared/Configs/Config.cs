using AutoClash.Shared.Clash;

namespace AutoClash.Shared.Configs;

public class Config
{
    /// <summary>
    /// 订阅服务
    /// </summary>
    public VpnProviderConfig[] VpnProviders { get; set; } = Array.Empty<VpnProviderConfig>();
    
    /// <summary>
    /// 自定义分组规则
    /// </summary>
    public CustomGroupConfig[] CustomGroups { get; set; } = Array.Empty<CustomGroupConfig>();

    /// <summary>
    /// 规则列表
    /// </summary>
    public string[] Rules { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// 远程规则集合
    /// </summary>
    public RuleSet[] RuleSets { get; set; }= Array.Empty<RuleSet>();
    
    /// <summary>
    /// gist 配置
    /// </summary>
    public GithubGistConfig GithubGist { get; set; }= new();
}