using System.Text;
using Newtonsoft.Json;
using Serilog;

namespace AutoClash.Console.Services;

public class GithubService: IGithubService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger _logger;

    public GithubService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _logger = Log.ForContext<GithubService>();
    }

    public async Task UpdateGist(string gistId, string fileName, string yaml)
    {
        var client = _clientFactory.CreateClient("github");

        var body = JsonConvert.SerializeObject(
            new
            {
                description = $"generate by auto-clash, at {DateTimeOffset.Now:yyyy-MM-ddTHH:mm:ss zz}",
                files = new Dictionary<string, object>
                {
                    { fileName, new { content = yaml } }
                }
            });
        
        var jsonContent = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"gists/{gistId}", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.Error("update gist file:{Gist} failed, response: {Res}", fileName, content);
        }
    }
}