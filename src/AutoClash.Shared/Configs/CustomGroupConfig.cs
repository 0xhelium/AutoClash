namespace AutoClash.Shared.Configs;

public class CustomGroupConfig
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string[] Filters { get; set; }
    
    public CustomGroupsConfigFilter[] GetFilters()
        => Filters.Select(x => new CustomGroupsConfigFilter(x)).ToArray();
}