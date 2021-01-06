namespace Husky.Core
{
    public static class HuskyConstants
    {
        public const string DefaultStageName = "Default Stage";
        public const string DefaultJobName = "Default Job";

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
    }
}