﻿using AutoClash.Shared.Clash;

namespace AutoClash.Shared.Configs;

public class Config
{
    /// <summary>
    /// 订阅服务
    /// </summary>
    public VpnProviderConfig[] VpnProviders { get; set; } = Array.Empty<VpnProviderConfig>();
    
    /// <summary>
    /// 分组生成规则
    /// </summary>
    public GroupGenerateRuleConfig[] GroupGenerateRules { get; set; } = Array.Empty<GroupGenerateRuleConfig>();
    
    /// <summary>
    /// 自定义分组规则
    /// </summary>
    public CustomGroupConfig[] CustomGroups { get; set; } = Array.Empty<CustomGroupConfig>();
    
    /// <summary>
    /// 规则列表
    /// </summary>
    public string[] Rules { get; set; }
    
    /// <summary>
    /// 远程规则集合
    /// </summary>
    public RuleSet[] RuleSets { get; set; }
    
    /// <summary>
    /// gist 配置
    /// </summary>
    public GithubGistConfig GithubGist { get; set; }
}