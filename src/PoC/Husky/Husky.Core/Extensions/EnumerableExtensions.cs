using System.Collections.Generic;

namespace Husky.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static string Csv<T>(this IEnumerable<T> @this) => string.Join(',', @this);
    }
}