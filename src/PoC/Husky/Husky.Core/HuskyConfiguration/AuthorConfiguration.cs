using FluentValidation;
using FluentValidation.Results;

namespace Husky.Core.HuskyConfiguration
{
    public partial record AuthorConfiguration: HuskyConfigurationBlock
    {
        public string Publisher { get; set; } = "Unknown Publisher";
        public string PublisherUrl { get; set; } = string.Empty;

        public override ValidationResult Validate() => new AuthorConfigurationValidator().Validate(this);

        private class AuthorConfigurationValidator: AbstractValidator<AuthorConfiguration>
        {
            public AuthorConfigurationValidator()
            {
                RuleFor(r => r.Publisher).NotNull();
                RuleFor(r => r.PublisherUrl).NotNull();
            }
        }
    }
}