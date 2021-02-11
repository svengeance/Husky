namespace Husky.Core
{
    public static class HuskyConstants
    {
        public static class RegistryKeys
        {
            public static class AppUninstalls
            {   // https://docs.microsoft.com/en-us/windows/win32/msi/uninstall-registry-key
                public const string RootKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                public const string DisplayName = nameof(DisplayName);
                public const string DisplayVersion = nameof(DisplayVersion);
                public const string Publisher = nameof(Publisher);
                public const string VersionMinor = nameof(VersionMinor);
                public const string VersionMajor = nameof(VersionMajor);
                public const string Version = nameof(Version);
                public const string HelpLink = nameof(HelpLink);
                public const string HelpTelephone = nameof(HelpTelephone);
                public const string InstallDate = nameof(InstallDate);
                public const string InstallLocation = nameof(InstallLocation);
                public const string InstallSource = nameof(InstallSource);
                public const string URLInfoAbout = nameof(URLInfoAbout);
                public const string URLUpdateInfo = nameof(URLUpdateInfo);
                public const string AuthorizedCDFPrefix = nameof(AuthorizedCDFPrefix);
                public const string Comments = nameof(Comments);
                public const string Contact = nameof(Contact);
                public const string EstimatedSize = nameof(EstimatedSize);
                public const string Language = nameof(Language);
                public const string ModifyPath = nameof(ModifyPath);
                public const string Readme = nameof(Readme);
                public const string UninstallString = nameof(UninstallString);
                public const string SettingsIdentifier = nameof(SettingsIdentifier);
                public const string NoModify = nameof(NoModify);
                public const string NoRemove = nameof(NoRemove);
                public const string NoRepair = nameof(NoRepair);

                // Not listed in the official documentation, but common
                public const string QuietUninstallString = nameof(QuietUninstallString);
            }
        }

        public static class UnixDesktopFileTypes
        {
            public const string Application = nameof(Application);
            public const string Link = nameof(Link);
            public const string Directory = nameof(Directory);
        }

        public static class UnixDesktopFileCategories
        {
            public static class Main
            {
                public const string AudioVideo = nameof(AudioVideo);
                public const string Audio = nameof(Audio);
                public const string Video = nameof(Video);
                public const string Development = nameof(Development);
                public const string Education = nameof(Education);
                public const string Game = nameof(Game);
                public const string Graphics = nameof(Graphics);
                public const string Network = nameof(Network);
                public const string Office = nameof(Office);
                public const string Science = nameof(Science);
                public const string Settings = nameof(Settings);
                public const string System = nameof(System);
                public const string Utility = nameof(Utility);
            }
        }

        public static class Workflows
        {
            public const string DefaultStageName = "default-stage";
            public const string DefaultJobName = "default-job";

            public static class StepTags
            {
                // All tasks by default should be able executable on Install and Uninstall
                public static readonly string[] DefaultStepTags = { Install, Uninstall };
                public const string Install = "install";
                public const string Repair = "repair";
                public const string Modify = "modify";
                public const string Uninstall = "uninstall";
            }

            public static class PreInstallation
            {
                public const string DefaultStageName = "husky-preinstallation-checks-stage";
                public const string DefaultJobName = "husky-preinstallation-checks-job";

                public static class Steps
                {
                    public const string VerifyClientMachineMeetsRequirements = "verify-client-machine-meets-requirements";
                }
            }

            public static class PostInstallation
            {
                public const string DefaultStageName = "husky-postinstallation-checks-stage";
                public const string DefaultJobName = "husky-postinstallation-checks-job";

                public static class Steps
                {
                    public const string PostInstallationApplicationRegistration = "post-installation-application-registration";
                }
            }
        }
    }
}