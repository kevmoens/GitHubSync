using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

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
            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "-h":
                    case "--h":
                    case "-?":
                    case "--?":
                        ShowHelp();
                        return;
                }
            }

            IConfigurationRoot config = LoadParameters(args);
            var help = config.GetSection("help");
            var settings = new SyncSettings()
            {
                Path = config["path"],
                User = config["user"],
                Organization = config["org"],
                CredentialID = string.IsNullOrEmpty(config["cred"]) ? "https://github.com" : config["cred"],
            };
            ValidateSettings(settings);

            var services = new ServiceCollection();
            RegisterServices(settings, services);
            var serviceProvider = services.BuildServiceProvider();

            var uploader = serviceProvider.GetRequiredService<GitRepoFinder>();
            uploader.Upload().GetAwaiter().GetResult();
        }

        private static IConfigurationRoot LoadParameters(string[] args)
        {
            var switchMappings = new Dictionary<string, string>()
            {
                { "--path", "path" },
                { "--user", "user" },
                { "--org", "org" },
                { "--cred", "cred" },
            };
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args, switchMappings);
            var config = builder.Build();
            return config;
        }

        private static void ValidateSettings(SyncSettings settings)
        {
            if (string.IsNullOrEmpty(settings.Path))
            {
                ShowHelp();
                throw new ArgumentNullException(nameof(settings.Path));
            }
            if (string.IsNullOrEmpty(settings.User))
            {
                ShowHelp();
                throw new ArgumentNullException(nameof(settings.User));
            }
            if (string.IsNullOrEmpty(settings.Organization))
            {
                ShowHelp();
                throw new ArgumentNullException(nameof(settings.Organization));
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("**GitHubSync Help**");
            Console.WriteLine("");
            Console.WriteLine("Use Windows Credential Manager to read in the https://github.com credentials\r\nhttps://lakecoaad-my.sharepoint.com/:w:/g/personal/kmoens_lakeco_com/EUh25H9eHOZJhQZQ3FccXiUB5hhwDaldt5BdFf2uwT1htg?e=vOjPH8");
            Console.WriteLine("");
            Console.WriteLine("--path = base directory that looks for sub directories that are git repositories");
            Console.WriteLine("--org = GitHub Organization (Place holding repos)");
            Console.WriteLine("--user = GitHub User (Cross reference to github user for permissions)");
            Console.WriteLine("--cred = [Optional] - Name of Windows Credential Manager Setting to get the GitHub token Default is https://github.com");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        private static void RegisterServices(SyncSettings settings, ServiceCollection services)
        {
            services.AddSingleton(settings);
            services.AddTransient<GitRepoFinder>();
            services.AddSingleton<GitHubRepositoryManager>();
            services.AddTransient<GitRepoUploader>();
        }
    }
}
