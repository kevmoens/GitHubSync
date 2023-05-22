using System;

namespace GitHubSync
{
    public class DisplayHelp : IDisplayHelp
    {
        public void ShowHelp()
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
    }
}
