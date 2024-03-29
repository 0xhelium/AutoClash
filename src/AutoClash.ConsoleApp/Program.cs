﻿using System.Net.Http.Headers;
using AutoClash.Console.Services;
using AutoClash.Shared.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Serilog;
using Serilog.Events;

namespace AutoClash.Console;

public class Program
{
    private static string[]? _args;
    private static readonly CancellationTokenSource _cts = new();
        
    public static async Task Main(string[] args)
    {
        _args = args;

        var loggerConf = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Information)
#else
            .MinimumLevel.Information()
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console();

        Log.Logger = loggerConf.CreateLogger();

        Log.ForContext<Program>().Debug("{State}", "Application starting");

        try
        {
            var token = _cts.Token;
            var builder = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(ServiceConfigure);

            var host = builder.Build();
            await host.RunAsync(token);
                
            Log.ForContext<Program>().Information("{State}","Application stopping");
        }
        catch (Exception e)
        {
            Log.Fatal(e.ToString());
        }
        finally
        {
            Log.Information("======== Application Stopped ========");
            await Log.CloseAndFlushAsync();
        }
    }

    private static void ServiceConfigure(HostBuilderContext ctx, IServiceCollection services)
    {
        Log.ForContext<Program>().Debug("{State}", "ServiceConfigure");

        AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy() => HttpPolicyExtensions
            .HandleTransientHttpError()            
            .WaitAndRetryAsync(
                10,
                _ => TimeSpan.FromSeconds(3),
                (result, ts) =>
                {
                    var response = result.Result;
                    var status = response.StatusCode;
                    var reqHost = response?.RequestMessage?.RequestUri?.Host;
                    var resp = response?.Content.ReadAsStringAsync().Result;

                    Log.Error("[retry in {S} seconds] request {Host} failed - {Status}, response: {Resp}", ts.Seconds, reqHost, status, resp);
                });
            
        services
            .AddHttpClient("default", client =>
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "ClashforWindows/0.20.39");
            })
            .AddPolicyHandler(req => GetRetryPolicy());

        services.AddHttpClient("github", (serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<Config>();
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", config.GithubGist.Token);
                client.DefaultRequestHeaders.Add("User-Agent", "auto-clash");
            })
            .AddPolicyHandler(req => GetRetryPolicy());
            
            
        services
            .AddSingleton(_cts)
            .AddHostedService<Worker>()
            .AddScoped<IProxyFetcher,ProxyFetcher>()
            .AddScoped<IGithubService,GithubService>()
            .AddSingleton<Config>( serviceProvider =>
            {
                var jsonConfUrl = GetUrlFromCmd(_args);
                    
                if (string.IsNullOrEmpty(jsonConfUrl))
                {
                    throw new Exception("argument `--config-url` not set");
                }

                var uri = new Uri(jsonConfUrl);
                var timespanQueryPrefix = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
                jsonConfUrl += $"{timespanQueryPrefix}_={DateTimeOffset.Now.Ticks}";
                var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("default");
                
                Log.Information(" > fetching config file");
                
                var response = httpClient.GetStringAsync(jsonConfUrl).Result;
                
                var config = JsonConvert.DeserializeObject<Config>(response) ?? new Config();
                return config;
            });

    }

    private static string GetUrlFromCmd(IEnumerable<string>? args)
    {
        var argList = args?.ToList()??new List<string>();
        const string cmdName = "--config-url";
        if (argList.All(x => x != cmdName)) return "";

        var index = argList.IndexOf(cmdName);
        return argList.Count > index + 1 ? argList[index + 1] : "";
    }
}