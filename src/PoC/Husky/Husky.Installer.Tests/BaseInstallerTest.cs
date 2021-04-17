﻿using System.IO;
using System.Reflection;
using Husky.Core.Workflow;
using NUnit.Framework;
using Husky.Core.HuskyConfiguration;
using Husky.Installer.Lifecycle;
using Husky.Internal.Generator.Dictify;

namespace Husky.Installer.Tests
{
    /*
     * Todo: Replace this class being an IntegrationTest with a new suite of tests - DockerIntegrationTests
     *       These tests will spin up docker containers and execute inside.
     *
     *       https://github.com/svengeance/Husky/issues/4
     *
     *       She won't be quick, but damnit if I'm going to start integration testing Registry changes on my desktop.
     *
     * 4/15/2021: As a follow-up, though the above is done, we still want a suite of integration tests that - can - test
     * the integration of the full workflow without necessarily spinning up the e2e container, which is expensive.
     */
    internal abstract class BaseInstallerTest
    {
        protected HuskyWorkflow Workflow { get; private set; } = null!;

        protected HuskyInstaller Installer { get; private set; } = null!;

        protected HuskyUninstaller Uninstaller { get; private set; } = null!;

        protected HuskyInstallerSettings InstallerSettings { get; private set; } = null!;

        protected abstract void ConfigureTestTaskOptions(TestHuskyTaskOptions options);

        private string _installPath = null!;

        [SetUp]
        public void BaseSetup()
        {
            ObjectFactory.AddFactory(typeof(TestHuskyTaskOptions), _ => new TestHuskyTaskOptions());
            _installPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Workflow = HuskyWorkflow.Create()
                                    .AddGlobalVariable("random.RandomNumber", "4")
                                    .Configure<InstallationConfiguration>(install => install.AddToRegistry = false)
                                    .Configure<ApplicationConfiguration>(app =>
                                     {
                                         app.Name = "TestApp";
                                         app.InstallDirectory = _installPath;
                                     })
                                    .WithDefaultStageAndJob(job =>
                                         job.AddStep<TestHuskyTaskOptions>("TestStep", ConfigureTestTaskOptions))
                                    .Build();

            InstallerSettings = new HuskyInstallerSettings { ResolveModulesFromAssemblies = new[] { Assembly.GetExecutingAssembly() } };
            InstallerSettings.LoadFromStartArgs(new[] { "install" });

            Installer = new HuskyInstaller(Workflow, InstallerSettings);
            Uninstaller = new HuskyUninstaller(Workflow, InstallerSettings);
        }

        [TearDown]
        public void BaseTearDown()
        {
            if (File.Exists(_installPath))
                File.Delete(_installPath);
        }
    }
}