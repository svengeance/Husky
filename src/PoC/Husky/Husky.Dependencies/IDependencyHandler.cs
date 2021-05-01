using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Husky.Core.Dependencies;
using Husky.Core.HuskyConfiguration;

namespace Husky.Dependencies
{
    public interface IDependencyHandler
    {
        bool TrySatisfyDependency([NotNullWhen(true)] out IDependencyAcquisitionMethod<HuskyDependency>? acquisitionMethod);

        ValueTask<bool> IsAlreadyInstalled();
    }
}