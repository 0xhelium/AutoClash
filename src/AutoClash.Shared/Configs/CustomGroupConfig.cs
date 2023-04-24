namespace AutoClash.Shared.Configs;

public class CustomGroupConfig
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string[] Filters { get; set; } = Array.Empty<string>();
    
    public CustomGroupsConfigFilter[] GetFilters()
        => Filters.Select(x => new CustomGroupsConfigFilter(x)).ToArray();
}