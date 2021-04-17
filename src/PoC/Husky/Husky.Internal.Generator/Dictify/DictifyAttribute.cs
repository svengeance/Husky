using System;

namespace Husky.Internal.Generator.Dictify
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DictifyAttribute: Attribute
    {
        private readonly bool _applyToDerivedClasses;
        private readonly string _portionToRemove;

        public DictifyAttribute(bool applyToDerivedClasses = false, string portionToRemove = null)
        {
            _applyToDerivedClasses = applyToDerivedClasses;
            _portionToRemove = portionToRemove;
        }
    }
}