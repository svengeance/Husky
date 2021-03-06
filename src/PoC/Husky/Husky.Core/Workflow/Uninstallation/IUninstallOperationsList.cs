using System.Collections.Generic;
using System.Threading.Tasks;

namespace Husky.Core.Workflow.Uninstallation
{
    public interface IUninstallOperationsList
    {
        void AddEntry(UninstallOperationsList.EntryKind kind, string entry);
        IEnumerable<string> ReadEntries(UninstallOperationsList.EntryKind kind);
        Task Flush();
    }
}