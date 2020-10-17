namespace Husky.Core.HuskyConfiguration
{
    public class HuskyDependency
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public HuskyDependency(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}