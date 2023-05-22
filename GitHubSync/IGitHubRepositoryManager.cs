using System.IO;
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
