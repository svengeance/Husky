using FluentValidation.Results;

namespace Husky.Core.Workflow
{
    public abstract class HuskyTaskConfiguration
    {
        internal abstract ValidationResult Validate();
    }
}