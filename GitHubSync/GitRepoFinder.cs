using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GitHubSync
{
    public class GitRepoFinder : IGitRepoFinder
    {
        private readonly SyncSettings _settings;
        private List<string> _repos = new List<string>();
        public GitRepoFinder(SyncSettings settings)
        {
            _settings = settings;
        }
        public async Task<List<string>> GetRepositories()
        {
            _repos = new List<string>();

            var baseDir = new DirectoryInfo(_settings.Path);
            await ProcessSubDirectories(baseDir);

            return _repos;
        }

        private async Task ProcessSubDirectories(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                try
                {
                    var repoPath = Repository.Discover(subDir.FullName);
                    if (string.IsNullOrEmpty(repoPath))
                    {
                        await ProcessSubDirectories(subDir);
                        continue;
                    }
                    _repos.Add(repoPath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error {subDir.FullName} {ex.Message} {ex.StackTrace}");
                    Environment.ExitCode = -1;
                }
            }
        }
    }
}
