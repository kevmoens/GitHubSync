using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace GitHubSync
{
    public class InputParameterProcessing : IInputParameterProcessing
    {
        public string[] InputArgs { get; set; }

        private readonly IDisplayHelp _displayHelp;
        private readonly SyncSettings _syncSettings;
        private readonly ILogger<InputParameterProcessing> _logger;

        public InputParameterProcessing(IDisplayHelp displayHelp, SyncSettings syncSettings, ILogger<InputParameterProcessing> logger)
        {
            _displayHelp = displayHelp;
            _syncSettings = syncSettings;
            _logger = logger;
        }

        public void Process()
        {
            if (AskedForHelp())
            {
                Environment.Exit(0);
                return;
            }

            IConfigurationRoot config = LoadParameters();

            _syncSettings.Path = config["path"];
            _syncSettings.User = config["user"];
            _syncSettings.Organization = config["org"];
            _syncSettings.CredentialID = string.IsNullOrEmpty(config["cred"]) ? "https://github.com" : config["cred"];

            ValidateSettings();
        }

        private bool AskedForHelp()
        {
            if (InputArgs.Length == 0) 
            {
                _displayHelp.ShowHelp();
                _logger.LogWarning("No parameters passed in.");
                return true;
            }

            if (InputArgs.Length == 1)
            {
                switch (InputArgs[0])
                {
                    case "-h":
                    case "--h":
                    case "-?":
                    case "--?":
                        _displayHelp.ShowHelp();
                        return true;
                }
            }
            return false;
        }

        private IConfigurationRoot LoadParameters()
        {
            var switchMappings = new Dictionary<string, string>()
            {
                { "--path", "path" },
                { "--user", "user" },
                { "--org", "org" },
                { "--cred", "cred" },
            };
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(InputArgs, switchMappings);
            var config = builder.Build();
            return config;
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrEmpty(_syncSettings.Path))
            {
                _displayHelp.ShowHelp();
                _logger.LogWarning("Path is missing");
                throw new ArgumentNullException(nameof(_syncSettings.Path));
            }
            if (string.IsNullOrEmpty(_syncSettings.User))
            {
                _displayHelp.ShowHelp();
                _logger.LogWarning("User is missing");
                throw new ArgumentNullException(nameof(_syncSettings.User));
            }
            if (string.IsNullOrEmpty(_syncSettings.Organization))
            {
                _displayHelp.ShowHelp();
                _logger.LogWarning("Organization is missing");
                throw new ArgumentNullException(nameof(_syncSettings.Organization));
            }
            var baseDir = new DirectoryInfo(_syncSettings.Path);
            if (baseDir.Exists == false)
            {
                _logger.LogWarning("Path doesn't exist");
                throw new DirectoryNotFoundException($"Error Loading Path {_syncSettings.Path}");
            }
        }
    }
}
