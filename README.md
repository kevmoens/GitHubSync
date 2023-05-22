# GitHubSync

Run locally on Windows to clone git repositories into GitHub.

First in GitHub generate access token:
https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token

Second open Windows Credential Manager and Add a generic credential
https://support.microsoft.com/en-us/windows/accessing-credential-manager-1b5c916a-6a16-889f-8581-fc16e8165ac0

--path = base directory that looks for sub directories that are git repositories
--org = GitHub Organization (Place holding repos)
--user = GitHub User (Cross reference to github user for permissions)
--cred = [Optional] - Name of Windows Credential Manager Setting to get the GitHub token Default is https://github.com
