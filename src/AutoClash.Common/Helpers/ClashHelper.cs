namespace AutoClash.Common.Helpers;

public abstract class ClashHelper
{
    public static Dictionary<string, object> GetProxyGroupByType(string type)
    {
        var proxyGroup = new Dictionary<string, object>
        {
            {"type",type},
            {"proxies",new List<string>()}
        };

        if (type is "url-test" or "load-balance")
        {
            proxyGroup["url"] = "http://www.gstatic.com/generate_204";
            proxyGroup["interval"] = 300;
            proxyGroup["tolerance"] = 50;
            if (type == "load-balance")
            {
                proxyGroup["strategy"] = "round-robin";
            }
        }

        return proxyGroup;
    }
}