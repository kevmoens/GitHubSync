﻿using CredentialManagement;
using Octokit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GitHubSync
{
    public class GitHubRepositoryManager
    {
        private readonly SyncSettings _settings;
        private readonly GitHubClient _client;
        public GitHubRepositoryManager(SyncSettings settings)
        {
            _settings = settings;
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
            catch
            {
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