using LibGit2Sharp.Handlers;
using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubSync
{
    public class GitHubRepoUploader : IGitHubRepoUploader
    {
        private readonly SyncSettings _settings;
        private readonly IGitHubRepositoryManager _gitHubRepository;
        private Remote _remote;
        public GitHubRepoUploader(SyncSettings syncSettings, IGitHubRepositoryManager gitHubRepository)
        {
            _settings = syncSettings;
            _gitHubRepository = gitHubRepository;
        }
        private Repository Repo;

        public async Task UploadRepoToGitHub(string repoPath)
        {

            using (var localRepo = new Repository(repoPath))
            {
                DirectoryInfo dir = GetRepositoryPath(localRepo);
                string remoteUrl = $"https://github.com/{_settings.Organization}/{dir.Name}.git";
                Console.WriteLine($"Remote URL {remoteUrl}");
                await _gitHubRepository.FindOrCreateGitHubRepo(dir);
                Repo = localRepo;
                AddOrUpdateRemote(remoteUrl);
                UpdateOrCreateRemoteTrackingReference();
                PushTrackedBranchesToRemote();
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


        private void AddOrUpdateRemote(string remoteUrl)
        {
            _remote = GetRemoteByName(Repo.Network, remoteUrl);

            if (_remote != null)
            {
                return;
            }

            if (Repo.Network.Remotes.Any(r => r.Name == "github"))
            {
                Console.WriteLine($"Bad GitHub Remote existed in {Repo.Info.Path}");
                Repo.Network.Remotes.Remove("github");
            }
            Console.WriteLine($"Added GitHub Remote in {Repo.Info.Path}");
            _remote = Repo.Network.Remotes.Add("github", remoteUrl);

        }

        private static Remote GetRemoteByName(Network network, string remoteUrl)
        {
            foreach (var remote in network.Remotes)
            {
                if (remote.Url == remoteUrl)
                {
                    return remote;
                }
            }

            return null;
        }

        private void PushTrackedBranchesToRemote()
        {
            var pushOptions = new PushOptions
            {
                CredentialsProvider = new CredentialsHandler(
                (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = _settings.User,
                        Password = _gitHubRepository.Password
                    })
            };
            foreach (var branch in Repo.Branches.Where(b => b.IsRemote && b.RemoteName == "github"))
            {
                //Allow Force Push using + on the RefSpec
                string pushRefSpec = string.Format("+{0}:{0}", branch.CanonicalName.Replace("refs/remotes/github/", "refs/heads/"));
                Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} Pushing {Repo.Info.Path}  {branch.FriendlyName}");
                Repo.Network.Push(_remote, pushRefSpec, pushOptions);
                Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} Pushed {Repo.Info.Path}  {branch.FriendlyName}");
            }
        }

        private void UpdateOrCreateRemoteTrackingReference()
        {
            foreach (var branch in Repo.Branches.Where(b => !b.IsRemote))
            {
                var trackingRef = "refs/heads/" + branch.FriendlyName;
                var remoteRef = "refs/remotes/" + _remote.Name + "/" + branch.FriendlyName;

                if (Repo.Refs.Any(r => r.CanonicalName == remoteRef))
                {
                    Repo.Refs.Remove(remoteRef);
                }

                Repo.Refs.Add(remoteRef, new ObjectId(branch.Tip.Sha));
                Repo.Refs.UpdateTarget(trackingRef, branch.Tip.Sha, "Created by LibGit2Sharp");
            }
        }
    }
}
