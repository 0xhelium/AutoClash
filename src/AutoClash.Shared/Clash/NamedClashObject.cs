using AutoClash.Common.Helpers;

namespace AutoClash.Shared.Clash;

public abstract class NamedClashObject: Dictionary<string,object>
{
    public NamedClashObject(Dictionary<string,object> source)
    {
        foreach (var (key, value) in source)
        {
            TryAdd(key,value);
        }
    }

    public string Name
    {
        get => this.TryGetValue("name", out var name) ? name.ToString()! : throw new Exception("`name` not found");
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
        if (!tag.Contains(',')) return TagHelper.HasTag(this, tag);
        
        var arr = tag.Split(',');
        return arr.All(t => TagHelper.HasTag(this, t));
    }
}