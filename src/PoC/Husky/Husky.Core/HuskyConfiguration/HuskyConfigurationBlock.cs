using FluentValidation.Results;

namespace Husky.Core.HuskyConfiguration
{
    public abstract record HuskyConfigurationBlock
    {
        public abstract ValidationResult Validate();
    }
}