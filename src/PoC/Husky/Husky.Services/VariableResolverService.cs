using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FastMember;
using Microsoft.Extensions.Logging;

namespace Husky.Services
{
    public interface IVariableResolverService
    {
        /// <summary>
        ///     Resolves all public string properties on an object, replacing any occurrences of a known variable in the string with said variable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object whose string properties will be replaced with resolved variables</param>
        /// <param name="variableSources">All possible variable sources to resolve from</param>
        void Resolve<T>(T obj, params IReadOnlyDictionary<string, string>[] variableSources);
    }
    
    public class VariableResolverService: IVariableResolverService
    {
        private readonly ILogger _logger;
        private static readonly Regex _varMatchingRegex = new(@"(?<!{){(\w|\.)+}");

        public VariableResolverService(ILogger<VariableResolverService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc cref="IVariableResolverService"/>
        public void Resolve<T>(T obj, params IReadOnlyDictionary<string, string>[] variableSources)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            _logger.LogDebug("Attempting to resolve variables on object {object}", obj.GetType().Name);

            /*
             * Todo: We're not going to be able to use FastMember and
             *       TypeAccessor here because of the runtime IL Generation, which is incompatible
             *       with any sort of Native AoT work we want to do.
             */
            var accessor = TypeAccessor.Create(obj.GetType());
            var stringProperties = accessor.GetMembers()
                                           .Where(w => w.Type == typeof(string))
                                           .Select(s => s.Name);

            foreach (var property in stringProperties) 
            {
                var value = accessor[obj, property]?.ToString();

                if (value == null)
                    continue;

                var variablesToReplace = _varMatchingRegex.Matches(value)
                                                          .Select(s => s.Value)
                                                          .ToArray();

                _logger.LogDebug("Replacing variables {variables}", variablesToReplace);
                var variableValues = variablesToReplace.Select(s => variableSources.Select(s2 => s2.TryGetValue(SanitizeVariableName(s), out var found) ? found : string.Empty)
                                                                                   .FirstOrDefault(f => f != string.Empty))
                                                       .ToList();

                _logger.LogDebug("Loading values {values}", variableValues);
                var sb = new StringBuilder(value);
                for (var i = 0; i < variableValues.Count; i++)
                {
                    var variableValue = variableValues[i];

                    if (string.IsNullOrEmpty(variableValue))
                        throw new ArgumentException($"Unable to locate variable {variablesToReplace[i]} inside property {property} of object {typeof(T).Name}");

                    sb.Replace(variablesToReplace[i], variableValue);
                }

                _logger.LogTrace("Setting {property} to {value}", property, sb.ToString());
                accessor[obj, property] = sb.ToString();
            }
        }

        private static string SanitizeVariableName(string name) => name.Trim('{', '}');
    }
}