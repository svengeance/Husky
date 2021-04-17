using FluentValidation.Results;
using Husky.Internal.Generator.Dictify;

namespace Husky.Core.HuskyConfiguration
{
    [Dictify(applyToDerivedClasses: true, portionToRemove: "Configuration")]
    public abstract record HuskyConfigurationBlock
    {
        public abstract ValidationResult Validate();
    }
}