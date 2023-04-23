using AutoClash.Common.Helpers;

namespace AutoClash.Shared.Clash;

public abstract class NamedClashObject: Dictionary<string,object>
{
    public NamedClashObject(Dictionary<string,object> source)
    {
        foreach (var (key, value) in source)
        {
            this.Remove(key);
            this.Add(key,value);
        }
    }

    public string Name
    {
        get => this.TryGetValue("name", out var name) ? name.ToString()! : throw new Exception("name not found");
        set
        {
            if (this.ContainsKey("name"))
            {
                this["name"] = value;
            }
            else
            {
                this.Add("name", value);
            }
        }
    }

    public void AddTag(string tag)
    {
        TagHelper.AddTag(this, tag);
    }
    
    public List<string> GetTags()
    {
        return TagHelper.GetTags(this).ToList();
    }

    public bool HasTag(string tag)
    {
        if (tag.Contains(','))
        {
            var result = true;
            var arr = tag.Split(',');
            foreach (var t in arr)
            {
               result = result && TagHelper.HasTag(this,t);
            }

            return result;
        }
        return TagHelper.HasTag(this,tag);
    }
}

public class Proxy : NamedClashObject
{
    public Proxy(Dictionary<string, object> source) : base(source)
    {
    }
}



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