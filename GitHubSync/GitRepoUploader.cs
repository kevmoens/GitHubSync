using LibGit2Sharp.Handlers;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync
{
    public class GitRepoUploader : IGitRepoUploader
    {
        private readonly SyncSettings _settings;
        private readonly IGitHubRepositoryManager _gitHubRepository;
        private Remote _remote;
        public GitRepoUploader(SyncSettings syncSettings, IGitHubRepositoryManager gitHubRepository)
        {
            _settings = syncSettings;
            _gitHubRepository = gitHubRepository;
        }
        public Repository Repo { get; set; }

        public void AddOrUpdateRemote(string remoteUrl)
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

        public void PushTrackedBranchesToRemote()
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

        public void UpdateOrCreateRemoteTrackingReference()
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
