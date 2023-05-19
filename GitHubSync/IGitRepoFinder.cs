using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync
{
    public interface IGitRepoFinder
    {
        Task Upload();
    }
}
