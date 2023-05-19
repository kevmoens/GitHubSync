using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync
{
    public interface IGitRepoUploader
    {
        Repository Repo { get; set; }
        void AddOrUpdateRemote(string remoteUrl);
        void PushTrackedBranchesToRemote();
        void UpdateOrCreateRemoteTrackingReference();
    }
}
