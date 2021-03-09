using System.Collections.Generic;
using System.Threading.Tasks;

namespace Husky.Core.Workflow.Uninstallation
{
    // A shim during uninstall operations such that executing user-defined uninstall operations do not themselves append to the file
    public sealed class ReadonlyUninstallOperationsList : IUninstallOperationsList
    {
        private readonly IUninstallOperationsList _operationsList;

        public ReadonlyUninstallOperationsList(IUninstallOperationsList operationsList)
        {
            _operationsList = operationsList;
        }

        public void AddEntry(UninstallOperationsList.EntryKind kind, string entry) { }

        public IEnumerable<string> ReadEntries(UninstallOperationsList.EntryKind kind) => _operationsList.ReadEntries(kind);

        public Task Flush() => Task.CompletedTask;
    }
}