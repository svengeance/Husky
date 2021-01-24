﻿using SemVer;

namespace Husky.Core.HuskyConfiguration
{
    public abstract record HuskyDependency
    {
        public Range ParsedRange { get; }

        private string Range => ParsedRange.ToString();

        protected HuskyDependency(string range) : this(new Range(range)) { }

        private HuskyDependency(Range range) => ParsedRange = range;
    }
}