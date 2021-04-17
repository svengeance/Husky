using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Husky.Core.Attributes;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.VisualStudio.TestAdapter.Internal;
using Serilog;

namespace Husky.Tests.Sdk
{
    [TestFixture]
    public abstract class HuskyTest: IDisposable
    {
        protected readonly ILogger Log;

        private MethodInfo? _huskyEntryPoint;

        protected HuskyTest()
        {
            SeriLogger.Initialize();
            Log = SeriLogger.Logger.ForContext(GetType());
            Log.Debug("Fixture Initialized");

            if (!Docker.IsInDockerContainer())
                Assert.Inconclusive("HuskyTests should not be executed outside of a Docker Container");
        }

        [SetUp]
        public void HuskySetup() => Log.Debug("Executing {testName}", TestContext.CurrentContext.Test.Name);

        public void ExecuteHuskyInstaller(string[] args)
        {
            _huskyEntryPoint ??= LocateHuskyEntryPoint();

            if (_huskyEntryPoint is null)
            {
                Log.Error("Unable to locate an EntryPoint for execution. No tests were executed.");
            } else
            {
                Log.Debug("Executing loaded assembly with args {args}", args);
                _ = _huskyEntryPoint.Invoke(null, new[] { args })!;
            }
        }

        public void Dispose()
        {
            SeriLogger.Dispose();
        }

        public async Task WaitForDebugger(int seconds = 60)
        {
            await Task.WhenAny(Task.Run(async () =>
            {
                while (!Debugger.IsAttached)
                    await Task.Delay(50);
            }), Task.Delay(TimeSpan.FromSeconds(seconds)));
        }

        private MethodInfo? LocateHuskyEntryPoint()
        {
            Log.Debug("Attempting to locate HuskyEntryPoint in {directory}", Directory.GetCurrentDirectory());
            var assembliesToScan = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll");
            MethodInfo? huskyEntryPoint = null;
            foreach (var assemblyFile in assembliesToScan)
            {
                var assembly = Assembly.LoadFrom(assemblyFile);
                if (assembly.EntryPoint is { } entryPoint &&
                    assembly.ExportedTypes.Any(a => a.GetMethods().Any(a2 => a2.GetCustomAttributes().Any(a3 => a3.GetType().Name == nameof(HuskyEntryPointAttribute)))))
                {
                    if (huskyEntryPoint is not null)
                        throw new InvalidOperationException(
                            $"Located multiple HuskyEntryPoints for Test Execution. Please ensure this test suite references only one installer." +
                            $"Located EntryPoints at {huskyEntryPoint.Module.FullyQualifiedName} and {entryPoint.Module.FullyQualifiedName}");

                    huskyEntryPoint = entryPoint;
                }
            }

            return huskyEntryPoint;
        }
    }
}