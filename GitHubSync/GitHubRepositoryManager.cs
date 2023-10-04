using CredentialManagement;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GitHubSync
{
    public class GitHubRepositoryManager : IGitHubRepositoryManager
    {
        private readonly SyncSettings _settings;
        private readonly ILogger<GitHubRepositoryManager> _logger;
        private readonly GitHubClient _client;
        public GitHubRepositoryManager(SyncSettings settings, ILogger<GitHubRepositoryManager> logger)
        {
            _settings = settings;
            _logger = logger;
            _client = new GitHubClient(new ProductHeaderValue("repo"));
        }
        public string Password { get; set; }
        public void LoadGitHubCredentials()
        {
            using (var cred = new Credential())
            {
                cred.Target = _settings.CredentialID;
                cred.Load();
                Password = cred.Password;
                var tokenAuth = new Credentials(cred.Password);
                _client.Credentials = tokenAuth;
            }

        }

        public async Task FindOrCreateGitHubRepo(DirectoryInfo dir)
        {
            try
            {
                await _client.Repository.Get(_settings.Organization, dir.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error finding repo {_settings.Organization}/{dir.Name}");
                Console.WriteLine($"Creating new GitHub Repo {dir.Name}");
                var newRepo = new NewRepository(dir.Name)
                {
                    Private = true
                };

                await _client.Repository.Create(newRepo);
            }
        }

    }
}
