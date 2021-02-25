using System;
using System.Threading.Tasks;
using Husky.Core.HuskyConfiguration;
using Range = SemVer.Range;

namespace Husky.Dependencies
{
    public interface IDependencyAcquisitionMethod<out T> where T : HuskyDependency
    {
        ValueTask AcquireDependency(IServiceProvider serviceProvider);

        /// <summary>
        ///     Determines whether or not the <see cref="SemVer.Range" /> of the passed in <see cref="HuskyDependency" /> is satisfied by
        ///     this <see cref="DependencyAcquisitionMethod{T}" />
        /// </summary>
        /// <param name="otherDependency">The dependency to check if satisfied</param>
        /// <returns>
        ///     True when this <see cref="DependencyAcquisitionMethod{T}" /> satisfies
        ///     <paramref name="otherDependency" />
        /// </returns>
        bool SatisfiesDependency(HuskyDependency otherDependency);
    }

    /// <summary>
    /// Represents <b>a</b> way to acquire the represented <see cref="Dependency"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DependencyAcquisitionMethod<T>: IDependencyAcquisitionMethod<T> where T : HuskyDependency
    {
        protected T Dependency { get; }

        protected DependencyAcquisitionMethod(T dependency) => Dependency = dependency;

        /*
         * Todo: Using ServiceLocator pattern here might not be the best, but I think is sufficient
         *       for the intended, short-lived nature of these DependencyAcquisitionMethods
         *
         */
        public abstract ValueTask AcquireDependency(IServiceProvider serviceProvider);

        /// <summary>
        ///     Determines whether or not the <see cref="SemVer.Range" /> of the passed in <see cref="HuskyDependency" /> is satisfied by
        ///     this <see cref="DependencyAcquisitionMethod{T}" />
        /// </summary>
        /// <param name="otherDependency">The dependency to check if satisfied</param>
        /// <returns>
        ///     True when this <see cref="DependencyAcquisitionMethod{T}" /> satisfies
        ///     <paramref name="otherDependency" />
        /// </returns>
        public bool SatisfiesDependency(HuskyDependency otherDependency)
            => Range.IsSatisfied(otherDependency.ParsedRange.ToString(), Dependency.ParsedRange.ToString());
    }
}