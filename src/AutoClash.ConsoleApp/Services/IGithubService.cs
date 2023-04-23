namespace AutoClash.Console.Services;

public interface IGithubService
{
    Task UpdateGist(string gistId, string fileName, string yaml);
}