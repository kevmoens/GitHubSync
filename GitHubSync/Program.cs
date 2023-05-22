using Microsoft.Extensions.DependencyInjection;

namespace GitHubSync
{
    internal class Program
    {
        /// <summary>
        /// 
        /// Use Windows Credential Manager to read in the https://github.com credentials
        /// https://lakecoaad-my.sharepoint.com/:w:/g/personal/kmoens_lakeco_com/EUh25H9eHOZJhQZQ3FccXiUB5hhwDaldt5BdFf2uwT1htg?e=vOjPH8
        /// 
        /// Pass in the 3 parameters
        ///   Path = base directory that looks for sub directories that are git repositories
        ///   Org = GitHub Organization (Place holding repos)
        ///   User = GitHub User (Cross reference to github user for permissions)
        ///   Cred = Optional - Name of Windows Credential Manager Setting to get the GitHub token
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            var serviceProvider = services.BuildServiceProvider();

            StartGitHubSync(args, serviceProvider);
        }

        private static void RegisterServices(ServiceCollection services)
        {
            var settings = new SyncSettings();
            services.AddSingleton(settings);
            services.AddTransient<IDisplayHelp, DisplayHelp>();
            services.AddTransient<IGitRepoFinder, GitRepoFinder>();
            services.AddSingleton<IGitHubRepositoryManager, GitHubRepositoryManager>();
            services.AddTransient<IGitHubRepoUploader, GitHubRepoUploader>();
        }

        private static void StartGitHubSync(string[] args, ServiceProvider serviceProvider)
        {
            var gitHubRepositoryManager = serviceProvider.GetRequiredService<IGitHubRepositoryManager>();
            gitHubRepositoryManager.LoadGitHubCredentials();

            var inputProcessing = serviceProvider.GetRequiredService<IInputParameterProcessing>();
            inputProcessing.InputArgs = args;
            inputProcessing.Process();

            var gitFinder = serviceProvider.GetRequiredService<GitRepoFinder>();
            var repos = gitFinder.GetRepositories().GetAwaiter().GetResult();

            foreach (var repo in repos)
            {
                var uploader = serviceProvider.GetRequiredService<IGitHubRepoUploader>();
                uploader.UploadRepoToGitHub(repo).GetAwaiter().GetResult();
            }
        }
    }
}
