namespace AutoClash.Common.Helpers;

public abstract class TagHelper
{
    private const string Tag = "__tag__";

    public static void AddTag(Dictionary<string,object> source, string value)
    {
        var tag = source.TryGetValue(Tag, out var tagContent);
        var newTag = (tagContent == null || string.IsNullOrEmpty(tagContent.ToString()))
            ? value
            : tagContent + "," + value;
        source.Remove(Tag);
        source.TryAdd(Tag, newTag);
    }

    public static string[] GetTags(Dictionary<string, object>source)
    {
        var tagContent = source.TryGetValue(Tag, out var value) ? value.ToString()! : "";
        return tagContent.Split(",");
    }

    public static bool HasTag(Dictionary<string, object> source, string tag)
    {
        var tags = GetTags(source);
        return tags.Any(x => x == tag);
    }
}