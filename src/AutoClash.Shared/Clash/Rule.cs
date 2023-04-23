namespace AutoClash.Shared.Clash;

public class Rule
{
    public Rule(string rule)
    {
        var arr = rule.Split(',');
        if (rule.StartsWith("MATCH") && arr.Length==2)
        {
            Type = "MATCH";
            Content = "";
            Proxy = arr[1];
            return;
        }
        
        Type = arr[0];
        Content = arr[1];
        Proxy = arr[2];
    }
    
    public string Type { get; set; }
    
    public string Content { get; set; }
    
    public string Proxy { get; set; }

    public override string ToString()
    {
        return $"{Type}{(string.IsNullOrEmpty(Content) ? "" : $",{Content}")},{Proxy}";
    }
}