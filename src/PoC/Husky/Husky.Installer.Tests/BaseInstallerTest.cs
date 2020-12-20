using System;
using System.Reflection;
using Husky.Core.Builder;
using Husky.Core.Workflow;
using NUnit.Framework;

namespace Husky.Installer.Tests
{
    public abstract class BaseInstallerTest
    {
        protected HuskyWorkflow Workflow { get; private set; } = null!;

        protected HuskyInstaller Installer { get; private set; } = null!;

        protected abstract void ConfigureTestTaskOptions(TestHuskyTaskOptions options);

        [SetUp]
        public void BaseSetup()
        {
            Workflow = HuskyWorkflow.Create()
                                    .AddGlobalVariable("random.RandomNumber", "4")
                                    .WithDefaultStageAndJob(job =>
                                         job.AddStep<TestHuskyTaskOptions>("TestStep", ConfigureTestTaskOptions)).Build();

            Installer = new HuskyInstaller(Workflow, cfg =>
            {
                cfg.ResolveModulesFromAssemblies = new[] { Assembly.GetExecutingAssembly() };
            });
        }
    }
}