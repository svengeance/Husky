using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Infrastructure;
using Husky.Core.TaskOptions.Installation;
using Husky.Core.TaskOptions.Resources;
using Husky.Core.TaskOptions.Scripting;
using Husky.Core.TaskOptions.Uninstallation;
using Husky.Core.TaskOptions.Utilities;
using Husky.Core.Workflow;
using Husky.Internal.Generator.Dictify;
using Husky.Services;
using Husky.Services.Infrastructure;
using Husky.Tasks.Installation;
using Husky.Tasks.Resources;
using Husky.Tasks.Scripting;
using Husky.Tasks.Uninstallation;
using Husky.Tasks.Utilities;
using StrongInject;

// Oh the price we pay to rid us of MS. Ext. DI
namespace Husky.Tasks.Infrastructure
{
    [Register(typeof(CreateScriptFile))]
    [Register(typeof(CreateShortcut))]
    [Register(typeof(ExecuteInlineScript))]
    [Register(typeof(ExecuteUninstallationOperations))]
    [Register(typeof(ExtractBundledResource))]
    [Register(typeof(PostInstallationApplicationRegistration))]
    [Register(typeof(VerifyMachineMeetsRequirements))]
    [RegisterModule(typeof(HuskyServicesModule))]
    [RegisterModule(typeof(HuskyCoreModule))]
    public partial class HuskyTasksContainer:
        IContainer<CreateScriptFile>,
        IContainer<CreateShortcut>,
        IContainer<ExecuteInlineScript>,
        IContainer<ExecuteUninstallationOperations>,
        IContainer<ExtractBundledResource>,
        IContainer<PostInstallationApplicationRegistration>,
        IContainer<VerifyMachineMeetsRequirements>
    {
        [Factory]
        private ApplicationConfiguration CreateAppConfiguration() => (ApplicationConfiguration)ObjectFactory.Create(typeof(ApplicationConfiguration), _context.Variables);

        [Factory]
        private InstallationConfiguration CreateInstallationConfiguration() => (InstallationConfiguration)ObjectFactory.Create(typeof(InstallationConfiguration), _context.Variables);

        [Factory]
        private ClientMachineRequirementsConfiguration CreateClientMachineRequirementsConfiguration() => (ClientMachineRequirementsConfiguration)ObjectFactory.Create(typeof(ClientMachineRequirementsConfiguration), _context.Variables);

        [Factory]
        private AuthorConfiguration CreateAuthorConfiguration() => (AuthorConfiguration)ObjectFactory.Create(typeof(AuthorConfiguration), _context.Variables);

        private readonly HuskyContext _context;

        private static Dictionary<Type, Func<object>> TaskFactoryByConfigurationType { get; } = new();

        public HuskyTasksContainer(HuskyContext context)
        {
            _context = context;
        }

        public Owned<HuskyTask<HuskyTaskConfiguration>> ResolveTaskForConfiguration(Type configurationType)
        {
            object foundTask;

            if (configurationType == typeof(CreateScriptFileOptions))
                foundTask = this.Resolve<CreateScriptFile>();
            else if (configurationType == typeof(CreateShortcutOptions))
                foundTask = this.Resolve<CreateShortcut>();
            else if (configurationType == typeof(ExecuteInlineScriptOptions))
                foundTask = this.Resolve<ExecuteInlineScript>();
            else if (configurationType == typeof(ExecuteUninstallationOperationsOptions))
                foundTask = this.Resolve<ExecuteUninstallationOperations>();
            else if (configurationType == typeof(ExtractBundledResourceOptions))
                foundTask = this.Resolve<ExtractBundledResource>();
            else if (configurationType == typeof(PostInstallationApplicationRegistrationOptions))
                foundTask = this.Resolve<PostInstallationApplicationRegistration>();
            else if (configurationType == typeof(VerifyMachineMeetsRequirementsOptions))
                foundTask = this.Resolve<VerifyMachineMeetsRequirements>();
            else if (TaskFactoryByConfigurationType.TryGetValue(configurationType, out var userDefinedFactory))
                foundTask = userDefinedFactory();
            else
                throw new ArgumentException($"Unable to resolve a task for type {configurationType}", nameof(configurationType));

            /* Todo: We currently have a "related type" issue, where we don't give a damn what type <T> is here, we just *know* it's a HuskyTaskConfiguration
            *  Unfortunately, the invariance on class-generic-types causes failures when trying to upcast T here, which is a *specific* configuration, to the base HTC.
            *  This *can* cause issues if we were to try to send *ANY OTHER* type other than the related type (i.e. send in Task1Configuration to a Task2)
            *  However, since I am only resolving and using the related type (i.e. we will *only* ever set Task1Configuration on Task1 here), this is somewhat safe
            *  In short, it may behoove us to get away from this if a different approach works better.
            */

            return Unsafe.As<Owned<HuskyTask<HuskyTaskConfiguration>>>(foundTask);
        }

        public static void RegisterCustomTask(Type configurationType, Func<object> factory) => TaskFactoryByConfigurationType[configurationType] = factory;
    }
}