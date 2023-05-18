using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubSync
{
    public class GitRepoFinder
    {
        private readonly SyncSettings _settings;
        private readonly GitHubRepositoryManager _gitHubRepository;
        private readonly GitRepoUploader _gitRepoUploader;
        public GitRepoFinder(SyncSettings settings, GitHubRepositoryManager gitHubRepository, GitRepoUploader gitRepoUploader)
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
            } catch (Exception ex)
            {
                Console.Error.WriteLine($"Error Loading GitHub Credentials {ex.Message} {ex.StackTrace}");
                Environment.ExitCode = -1;
                return;
            }
            var baseDir = new DirectoryInfo(_settings.Path);
            if (baseDir.Exists == false)
            {
                Console.Error.WriteLine($"Error Loading Path {_settings.Path}");
                Environment.ExitCode = -1;
                return;
            }
            foreach (var subDir in baseDir.GetDirectories())
            {
                try
                {
                    var repoPath = Repository.Discover(subDir.FullName);
                    if (string.IsNullOrEmpty(repoPath))
                    {
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
            DirectoryInfo dir = null;
            if (localRepo.Info.IsBare)
            {
                dir = new DirectoryInfo(localRepo.Info.Path);
            }
            else
            {
                dir = new DirectoryInfo(localRepo.Info.WorkingDirectory);
            }

            return dir;
        }
    }
}
