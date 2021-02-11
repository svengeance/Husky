using System.Reflection;
using Husky.Core;
using Husky.Core.Workflow;
using NUnit.Framework;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;

namespace Husky.Installer.Tests
{
    /*
     * Todo: Replace this class being an IntegrationTest with a new suite of tests - DockerIntegrationTests
     *       These tests will spin up docker containers and execute inside.
     *
     *       https://github.com/svengeance/Husky/issues/4
     *
     *       She won't be quick, but damnit if I'm going to start integration testing Registry changes on my desktop.
     */
    public abstract class BaseInstallerTest
    {
        protected HuskyWorkflow Workflow { get; private set; } = null!;

        protected HuskyInstaller Installer { get; private set; } = null!;

        protected HuskyInstallerSettings InstallerSettings { get; private set; } = null!;

        protected abstract void ConfigureTestTaskOptions(TestHuskyTaskOptions options);

        [SetUp]
        public void BaseSetup()
        {
            Workflow = HuskyWorkflow.Create()
                                    .AddGlobalVariable("random.RandomNumber", "4")
                                    .Configure<InstallationConfiguration>(install => install.AddToRegistry = false)
                                    .WithDefaultStageAndJob(job =>
                                         job.AddStep<TestHuskyTaskOptions>("TestStep", ConfigureTestTaskOptions))
                                    .Build();

            InstallerSettings = new HuskyInstallerSettings { ResolveModulesFromAssemblies = new[] { Assembly.GetExecutingAssembly() } };
            InstallerSettings.LoadFromStartArgs(new[] { "install" });

            Installer = new HuskyInstaller(Workflow, InstallerSettings);
        }
    }
}