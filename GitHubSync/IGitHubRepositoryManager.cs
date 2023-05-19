using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync
{
    public interface IGitHubRepositoryManager
    {
        string Password { get; set; }

        void LoadGitHubCredentials();
        Task FindOrCreateGitHubRepo(DirectoryInfo dir);
    }
}
