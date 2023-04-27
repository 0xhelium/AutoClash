namespace AutoClash.Shared.Clash;

public class ProxyGroup : NamedClashObject
{
    public List<string> Proxies
    {
        get => this.TryGetValue("proxies", out var ps) 
            ? (List<string>)ps
            : new List<string>();
        set
        {
            if (this.ContainsKey("proxies"))
            {
                this["proxies"] = value;
            }
            else
            {
                this.Add("proxies", value);
            }
        }
    }

    public ProxyGroup(Dictionary<string, object> source) : base(source)
    {
    }
}