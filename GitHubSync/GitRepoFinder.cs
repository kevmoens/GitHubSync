using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

namespace GitHubSync
{
    public class GitRepoFinder : IGitRepoFinder
    {
        private readonly SyncSettings _settings;
        private readonly IGitHubRepositoryManager _gitHubRepository;
        private readonly IGitRepoUploader _gitRepoUploader;
        public GitRepoFinder(SyncSettings settings, IGitHubRepositoryManager gitHubRepository, IGitRepoUploader gitRepoUploader)
        {
            _settings = settings;
            _gitHubRepository = gitHubRepository;
            _gitRepoUploader = gitRepoUploader;
        }
        public async Task Upload()
        {
            try
            {
                _gitHubRepository.LoadGitHubCredentials();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error Loading GitHub Credentials {ex.Message} {ex.StackTrace}");
                Environment.ExitCode = -1;
                return;
            }

            var baseDir = new DirectoryInfo(_settings.Path);
            await ProcessSubDirectories(baseDir);
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
                    Console.WriteLine($"Starting {repoPath}");
                    await UploadRepoToGitHub(repoPath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error {subDir.FullName} {ex.Message} {ex.StackTrace}");
                    Environment.ExitCode = -1;
                }
            }
        }

        private  async Task UploadRepoToGitHub(string repoPath)
        {

            using (var localRepo = new Repository(repoPath))
            {
                DirectoryInfo dir = GetRepositoryPath(localRepo);
                string remoteUrl = $"https://github.com/{_settings.Organization}/{dir.Name}.git";
                Console.WriteLine($"Remote URL {remoteUrl}");
                await _gitHubRepository.FindOrCreateGitHubRepo(dir);
                _gitRepoUploader.Repo = localRepo;
                _gitRepoUploader.AddOrUpdateRemote(remoteUrl);
                _gitRepoUploader.UpdateOrCreateRemoteTrackingReference();
                _gitRepoUploader.PushTrackedBranchesToRemote();
            }
        }

        private DirectoryInfo GetRepositoryPath(Repository localRepo)
        {            
            if (localRepo.Info.IsBare)
            {
                return new DirectoryInfo(localRepo.Info.Path);
            }
            else
            {
                return new DirectoryInfo(localRepo.Info.WorkingDirectory);
            }
        }
    }
}
