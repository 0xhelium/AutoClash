namespace AutoClash.Shared.Configs;

public class GroupGenerateRuleConfig
{
    public string Name { get; set; }

    /// <summary>
    /// 按地区生成分组信息
    /// </summary>
    public bool GenCountryGroups { get; set; } = true;
}