using System.Collections.Generic;

namespace Husky.Internal.Generator.Dictify
{
    public interface IDictable
    {
        //public object FromDictionary(IReadOnlyDictionary<string, object> dict);

        public Dictionary<string, object> ToDictionary();
    }
}