using System;

namespace Husky.Generator.Extensions
{
    public static class StringExtensions
    {
        public static string CapitalizeFirstLetter(this string s)
            => char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}