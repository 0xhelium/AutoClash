namespace AutoClash.Shared.Clash;

public class RuleSet
{
    public string Type { get; set; } = "http";
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Behavior { get; set; } = string.Empty;

    public string Path => $"./ruleset/{Name}.yaml";

    public int Interval { get; set; } = 86400; //24h

    public Dictionary<string, object> ToDict()
    {
        return new Dictionary<string, object>
        {
            { "type", Type },
            { "url", Url },
            { "behavior", Behavior },
            { "path", Path },
            { "interval", Interval },
        };
    }
}