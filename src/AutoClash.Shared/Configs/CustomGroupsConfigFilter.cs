namespace AutoClash.Shared.Configs;

public class CustomGroupsConfigFilter
{
    public CustomGroupsConfigFilter(string content)
    {
        if (content == "DIRECT")
        {
            Type = Tag = "";
            Filter = "DIRECT";
            return;
        }

        var arr = content.Split("::");
        Type = arr[0];  
        Tag = arr[1];
        Filter = arr[2];

        if (Type != "proxy" && Type != "group")
        {
            throw new Exception($"`{content}` is invalid, `{Type}` is not allowed");
        }
    }

    /// <summary>
    /// 对应的是节点或者分组的 tag 信息
    /// </summary>
    public string Tag { get; private set; }
    
    /// <summary>
    /// proxy  group
    /// </summary>
    public string Type { get;private set; }
    
    /// <summary>
    /// 正则
    /// </summary>
    public string Filter { get; private set; }
}