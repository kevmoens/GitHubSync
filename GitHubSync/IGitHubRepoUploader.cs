using System.Threading.Tasks;

namespace GitHubSync
{
    public interface IGitHubRepoUploader
    {
        Task UploadRepoToGitHub(string repoPath);
    }
}
