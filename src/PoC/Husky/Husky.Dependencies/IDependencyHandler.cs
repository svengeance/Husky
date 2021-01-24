using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Husky.Core.HuskyConfiguration;

namespace Husky.Dependencies
{
    public interface IDependencyHandler<T> where T: HuskyDependency
    {
        bool TrySatisfyDependency(T dependency, [NotNullWhen(true)] out DependencyAcquisitionMethod<T>? acquisitionMethod);

        ValueTask<bool> IsAlreadyInstalled(T dependency);
    }
}