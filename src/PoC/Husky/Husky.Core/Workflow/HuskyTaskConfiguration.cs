using FluentValidation.Results;
using Husky.Internal.Generator.Dictify;

namespace Husky.Core.Workflow
{
    [Dictify(applyToDerivedClasses: true)]
    public abstract class HuskyTaskConfiguration
    {
        internal abstract ValidationResult Validate();
    }
}