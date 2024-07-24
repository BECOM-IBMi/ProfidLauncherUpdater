using FlintSoft.Result;
using Microsoft.Extensions.Logging;
using ProfidLauncherUpdater.Shared;
using System.Text.Json;

namespace ProfidLauncherUpdater.Features.General
{
    public class RemoteVersionService(InstallationConfigurationModel config, IHttpClientFactory httpClientFactory, ILogger<RemoteVersionService> logger)
    {
        private readonly InstallationConfigurationModel _config = config;
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
                    var response = await _client.GetAsync($"{_config.Repository.VersionPath}{_config.Repository.LauncherInfo.SoftwareId}", cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new Error(nameof(GetCurrentVersionFromServer) + ".StatusCode", "Couldn't load version from repository: " + response.StatusCode);
                    }

                    var repoStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var repo = await JsonSerializer.DeserializeAsync<RepositoryModel>(repoStream, cancellationToken: cancellationToken);

                    if (repo is null)
                    {
                        return new Error(nameof(GetCurrentVersionFromServer) + ".JSON", "Couldn't load json");
                    }

                    _currentVersion = repo;
                    _logger.LogInformation($"Found version {_currentVersion.LatestVersion} on the server!");
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
