using System.Text.RegularExpressions;
using AutoClash.Common.Helpers;
using AutoClash.Console.Services;
using AutoClash.Shared.Clash;
using AutoClash.Shared.Configs;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using YamlDotNet.Serialization;

namespace AutoClash.Console;

public class Worker : BackgroundService
{
    private readonly Dictionary<string, string> _countryDict;
    private IEnumerable<string> _countryNames;

    private readonly ILogger _logger;
    private readonly IProxyFetcher _proxyFetcher;
    private readonly IGithubService _githubService;
    private readonly Config _config;
    private CancellationTokenSource _cts;

    private List<Proxy> _proxies=new();
    private List<ProxyGroup> _proxyGroups=new();
    private List<Rule> _rules = new();
    private List<RuleSet> _ruleSets = new();

    public Worker(IProxyFetcher proxyFetcher, IGithubService githubService, Config config, CancellationTokenSource cts)
    {
        _proxyFetcher = proxyFetcher;
        _config = config;
        _cts = cts;
        _githubService = githubService;
        _logger = Log.ForContext<Worker>();

        var countryJson = Path.Combine(AppContext.BaseDirectory, "Dict", "Countries.json");
        if (!File.Exists(countryJson))
        {
            throw new Exception("`Dict/Countries.json` file not found.");
        }

        var fileContent = File.ReadAllText(countryJson);
        _countryDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent) ?? new Dictionary<string, string>();
        _countryNames = _countryDict.Keys.ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Init();
        var dict = await GenerateFinalYaml();
        
        _logger.Information("uploading config to gist");
        await UploadFinalConfig(dict);
        
        _logger.Information("======== finished! ========");
        _logger.Debug("cancelling application");
        _cts.Cancel();
    }

    private async Task Init()
    {
        _logger.Information("fetching proxies...");
        await GetAllProxies();
        
        _logger.Information("generating proxy groups by country...");
        await GenProxyGroupsByCountry();

        _logger.Information("generating proxy groups by config...");
        await GenCustomProxyGroups();
        
        _logger.Information("generating rules...");
        await GenRules();
        
        _logger.Information("generating rule providers...");
        await GenRuleProviders();
    }

    private Task<Dictionary<string,object>> GenerateFinalYaml()
    {
        var tag = "__tag__";
        _proxies.ForEach(x =>
        {
            x.Remove(tag);
        });
        _proxyGroups.ForEach(x =>
        {
            x.Remove(tag);
        });
        var ruleProviders = _ruleSets.ToDictionary(x => x.Name, x => x.ToDict());
        var rules = _rules.Select(x => x.ToString()).ToList();
        return Task.FromResult(new Dictionary<string, object>()
        {
            { "proxies", _proxies },
            { "proxy-groups", _proxyGroups },
            { "rules", rules },
            { "rule-providers", ruleProviders },
        });
    }

    /// <summary>
    /// 获取所有订阅的节点信息
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private async Task GetAllProxies()
    {
        if (_config.VpnProviders.Length == 0)
        {
            return ;
        }

        await Parallel.ForEachAsync(_config.VpnProviders, async (config, _) =>
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            
            var ps = await _proxyFetcher.GetProxies(config.Url);
            if (ps?.Count > 0)
            {
                foreach (var p in ps)
                {
                    p.AddTag(config.Name);
                }

                if (!string.IsNullOrEmpty(config.ExcludeFilter))
                {
                    var r = new Regex(config.ExcludeFilter);
                    _proxies.AddRange(ps.Where(p => !r.IsMatch(p.Name)));
                }
                else
                {
                    _proxies.AddRange(ps);
                }
            }
        });
    }

    /// <summary>
    /// 根据节点生成分组信息
    /// </summary>
    /// <returns></returns>
    private  Task GenProxyGroupsByCountry()
    {
        var providers = _config.VpnProviders;
        if (providers.Length == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var provider in providers)
        {
            var proxies = _proxies.Where(x => x.HasTag(provider.Name)).ToList();
            if (provider.GenCountryGroups)
            {
                var countryGroups = new Dictionary<string, List<string>>(); 
                foreach (var proxy in proxies)
                {
                    var proxyName = proxy.Name;
                    var countryName = _countryNames.FirstOrDefault(x => proxyName.Contains(x));
                    if (string.IsNullOrEmpty(countryName)) continue;

                    var groupName = $"{_countryDict[countryName]} {provider.Name}_{countryName}";
                    if (countryGroups.TryGetValue(groupName, out var groupItems))
                    {
                        groupItems.Add(proxyName);
                        countryGroups[groupName] = groupItems;
                    }
                    else
                    {
                        countryGroups.Add(groupName, new List<string> { proxyName });
                    }
                }

                if (countryGroups.Any())
                {
                    var cgs = countryGroups.Select(x =>
                    {
                        var proxyGroup = new ProxyGroup(ClashHelper.GetProxyGroupByType("url-test"));
                        proxyGroup.Name = x.Key;
                        proxyGroup.Proxies.AddRange(x.Value);
                        proxyGroup.AddTag(provider.Name);
                        proxyGroup.AddTag("area_group");
                        return proxyGroup;
                    }).ToList();
                    _proxyGroups.AddRange(cgs);
                }
            }
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 生成自定义分组信息
    /// </summary>
    /// <returns></returns>
    private Task GenCustomProxyGroups()
    {
        var customGroups = _config.CustomGroups;
        if (customGroups.Length == 0)
        {
            return Task.CompletedTask;
        }

        _proxyGroups.InsertRange(
            0,
            customGroups.Select(x =>
                new ProxyGroup(new()
                {
                    { "name", Guid.NewGuid().ToString() }
                })
            )
        );

        //var numberOfProcessed = 0;
        var processingIndex = -1;
        var processed = new List<int>();

        while (processed.Count < customGroups.Length)
        {
            processingIndex++;
            if (processingIndex == customGroups.Length)
            {
                processingIndex = 0;
            }
            
            var customGroup = customGroups[processingIndex];
            if (processed.Contains(processingIndex))
            {
                continue;
            }

            
            var filters = customGroup.GetFilters();
            if (filters.Length == 0) continue;

            var filteredProxies = new List<string>();
            var filteredTimes = 0;
            foreach (var filter in filters)
            {
                if (filter.Filter == "DIRECT")
                {
                    filteredProxies.Add("DIRECT");
                }

                var regex = new Regex(filter.Filter);
                var filteredNames = ((IEnumerable<NamedClashObject>)(filter.Type == "proxy" ? _proxies : _proxyGroups))
                    .Where(x => x.HasTag(filter.Tag) && regex.IsMatch(x.Name))
                    .Select(x => x.Name)
                    .ToList();

                if (filteredNames.Count == 0 && filter.Type == "group")
                {
                    //找不到分组时, 才中断当前查询
                    break;
                }

                filteredProxies.AddRange(filteredNames);
                filteredTimes++;
            }

            if (filteredTimes == filters.Length)
            {
                processed.Add(processingIndex);
            }
            else
            {
                continue;
            }

            if (!filteredProxies.Any()) continue;

            var proxyGroup = new ProxyGroup(ClashHelper.GetProxyGroupByType(customGroup.Type));
            proxyGroup.Name = customGroup.Name;
            proxyGroup.AddTag("custom");
            proxyGroup.Proxies = filteredProxies;

            _proxyGroups.RemoveAt(processingIndex);
            _proxyGroups.Insert(processingIndex, proxyGroup);
        }

        return Task.CompletedTask;
    }

    private Task GenRules()
    {
        _rules = _config.Rules.Select(x =>
        {
            var rule = new Rule(x);
            if (rule.Proxy!="DIRECT")
            {
                var group = _proxyGroups.FirstOrDefault(g => g.Name.Contains(rule.Proxy));
                if (group == null)
                {
                    var proxy= _proxies.FirstOrDefault(p => p.Name.Contains(rule.Proxy));
                    rule.Proxy = proxy?.Name ?? throw new Exception($"invalid rule:{rule}");
                }
                else
                {
                    rule.Proxy = group.Name;
                }
            }

            return rule;
        }).ToList();
        return Task.CompletedTask;
    }

    private Task GenRuleProviders()
    {
        var rules = _rules.Select(x => x.Content);
        var inuse = _config.RuleSets.Where(x => rules.Contains(x.Name));
        _ruleSets = inuse.ToList();
        return Task.CompletedTask;
    }

    private async Task UploadFinalConfig(object config)
    {
        var serializer = new SerializerBuilder()
            .Build();
        var yaml = serializer.Serialize(config);
        var reg = new Regex(@"\\U[A-F0-9]{8}");
        var replacedString = reg.Replace(yaml, match =>
        {
            var unicodeInt = Convert.ToInt32(match.Value[2..], 16);
            var emojiString = char.ConvertFromUtf32(unicodeInt);
            return emojiString;
        });

        await _githubService.UpdateGist(_config.GithubGist.GistId, _config.GithubGist.FileName, replacedString);
    }
}