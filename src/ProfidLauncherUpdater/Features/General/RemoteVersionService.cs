using ProfidLauncherUpdater.Shared;
using System.Text.Json;

namespace ProfidLauncherUpdater.Features.General
{
    public class RemoteVersionService(InstallationConfigurationModel config, IHttpClientFactory httpClientFactory)
    {
        private readonly InstallationConfigurationModel _config = config;
        private readonly HttpClient _client = httpClientFactory.CreateClient("repo");
        private string? _currentVersion;

        public async Task<Result<string>> GetCurrentVersionFromServer()
        {
            try
            {
                if (_currentVersion is null)
                {
                    var response = await _client.GetAsync(_config.Repository.VersionFile);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new Error(nameof(GetCurrentVersionFromServer) + ".StatusCode", "Couldn't load version from repository: " + response.StatusCode);
                    }

                    var repoStream = await response.Content.ReadAsStreamAsync();
                    var repo = await JsonSerializer.DeserializeAsync<RepositoryModel>(repoStream);

                    if (repo is null)
                    {
                        return new Error(nameof(GetCurrentVersionFromServer) + ".JSON", "Couldn't load json");
                    }

                    _currentVersion = repo.Current;
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
