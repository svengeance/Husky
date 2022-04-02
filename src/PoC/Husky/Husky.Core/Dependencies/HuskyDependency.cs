using SemanticVersioning;

namespace Husky.Core.Dependencies
{
    public abstract record HuskyDependency
    {
        public Range ParsedRange { get; }

        protected HuskyDependency(string range) : this(new Range(range)) { }

        private HuskyDependency(Range range) => ParsedRange = range;
    }
}