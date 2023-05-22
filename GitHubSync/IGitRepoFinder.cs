using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHubSync
{
    public interface IGitRepoFinder
    {
        Task<List<string>> GetRepositories();
    }
}
