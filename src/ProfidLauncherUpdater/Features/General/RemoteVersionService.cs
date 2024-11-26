using FlintSoft.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProfidLauncherUpdater.Shared;
using System.Text.Json;

namespace ProfidLauncherUpdater.Features.General
{
    public class RemoteVersionService(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<RemoteVersionService> logger)
    {
        private readonly IConfiguration _config = config;
        private readonly HttpClient _client = httpClientFactory.CreateClient("repo");
        private RepositoryModel? _currentVersion;
        private readonly ILogger<RemoteVersionService> _logger = logger;

        public async Task<Result<RepositoryModel>> GetCurrentVersionFromServer(CancellationToken cancellationToken)
        {
            try
            {
                if (_currentVersion is null)
                {
                    _logger.LogInformation("Loading version from server...");
                    var launcherRepo = _config.GetValue<string>("installation:repository:launcher") ?? "";
                    var response = await _client.GetAsync($"{launcherRepo}", cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new Error(nameof(GetCurrentVersionFromServer) + ".StatusCode", "Couldn't load version from repository: " + response.StatusCode);
                    }

                    var repoStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var repo = await JsonSerializer.DeserializeAsync<ServerVersionModel>(repoStream, cancellationToken: cancellationToken);

                    if (repo is null)
                    {
                        return new Error(nameof(GetCurrentVersionFromServer) + ".JSON", "Couldn't load json");
                    }

                    //Die Version steht im Tag des Release
                    //The tag consists of vXX.YY.ZZ
                    var tag = repo.tag_name;
                    if (string.IsNullOrEmpty(tag))
                    {
                        return new Error(nameof(GetCurrentVersionFromServer) + "_TAG", "The version tag from the server is empty");
                    }

                    var version = tag.Substring(1, tag.Length - 1);
                    _currentVersion = new RepositoryModel()
                    {
                        VersionOnServer = version,
                        ServerVersion = repo
                    };

                    _logger.LogInformation($"Found version {_currentVersion.VersionOnServer} on the server!");
                }

                return _currentVersion;
            }
            catch (Exception ex)
            {
                return new Error(nameof(RemoteVersionService) + "." + nameof(GetCurrentVersionFromServer) + ".Error", "Error loading version from server: " + ex.Message);
            }
        }
    }
}
