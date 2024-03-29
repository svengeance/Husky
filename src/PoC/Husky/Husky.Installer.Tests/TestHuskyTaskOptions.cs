﻿using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;
using Husky.Internal.Generator.Dictify;

namespace Husky.Installer.Tests
{
    public partial class TestHuskyTaskOptions : HuskyTaskConfiguration, IDictable
    {
        public string Title { get; set; } = string.Empty;

        public bool HasValidated { get; set; } = false;

        internal override ValidationResult Validate()
        {
            HasValidated = true;
            return new TestHuskyTaskOptionsValidator().Validate(this);
        }

        private class TestHuskyTaskOptionsValidator : AbstractValidator<TestHuskyTaskOptions>
        {
            public TestHuskyTaskOptionsValidator()
            {
                RuleFor(r => r.Title).NotEmpty();
            }
        }

        public Dictionary<string, object> ToDictionary() => new()
        {
            ["TestHuskyTask.Title"] = Title,
            ["TestHuskyTask.HasValidated"] = HasValidated
        };
    }
}