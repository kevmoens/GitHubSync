using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GitHubSync
{
    internal class Program
    {
        private static ILogger<Program> _logger;

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

            _logger = serviceProvider.GetService<ILogger<Program>>();

            StartGitHubSync(args, serviceProvider);
        }

        private static void RegisterServices(ServiceCollection services)
        {
            var settings = new SyncSettings();
            services.AddSingleton(settings);
            services.AddTransient<IDisplayHelp, DisplayHelp>();
            services.AddTransient<IInputParameterProcessing, InputParameterProcessing>();
            services.AddTransient<IGitRepoFinder, GitRepoFinder>();
            services.AddSingleton<IGitHubRepositoryManager, GitHubRepositoryManager>();
            services.AddTransient<IGitHubRepoUploader, GitHubRepoUploader>();

            services.AddLogging(builder => {
                builder.SetMinimumLevel(LogLevel.Trace);                
                builder.AddNLog();
            });
        }

        private static void StartGitHubSync(string[] args, ServiceProvider serviceProvider)
        {
            try
            {
                var inputProcessing = serviceProvider.GetRequiredService<IInputParameterProcessing>();
                inputProcessing.InputArgs = args;
                inputProcessing.Process();
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing input parameters");
                return;
            }

            try
            {
                var gitHubRepositoryManager = serviceProvider.GetRequiredService<IGitHubRepositoryManager>();
                gitHubRepositoryManager.LoadGitHubCredentials();
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading GitHub Credentials");
                return;
            }

            List<string> repos = new List<string>();
            try
            {
                IGitRepoFinder gitFinder = serviceProvider.GetRequiredService<IGitRepoFinder>();
                repos = gitFinder.GetRepositories().GetAwaiter().GetResult();
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding git repositories");
                return;
            }

            foreach (var repo in repos)
            {
                try
                {
                    var uploader = serviceProvider.GetRequiredService<IGitHubRepoUploader>();
                    uploader.UploadRepoToGitHub(repo).GetAwaiter().GetResult();
                } catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error uploading {repo}");
                }
            }
        }
    }
}
