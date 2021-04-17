using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FastMember;
using Husky.Internal.Generator;
using Husky.Internal.Generator.Dictify;
using Microsoft.Extensions.Logging;

namespace Husky.Services
{
    public interface IVariableResolverService
    {
        /// <summary>
        /// Resolves a runtime type <paramref name="t"/> to an object through the use of <see cref="Dictable"/>.
        /// </summary>
        /// <remarks>
        /// The passed in type <b>must</b> be a <seealso cref="Dictable"/> type. This method will throw otherwise.
        /// <br />
        /// Execution of this method will also update all located variables in <paramref name="variableSource"/>,
        /// replacing located variables with their proper values.
        /// </remarks>
        /// <param name="t">The runtime type to resolve/</param>
        /// <param name="variableSource">The source of variables within which to resolve the properties of the type.</param>
        /// <returns></returns>
        object Resolve(Type t, Dictionary<string, object> variableSource);

        /// <summary>
        /// Resolves all variables within the <paramref name="variableSource"/>, updating
        /// its variable references to the resolved values.
        /// </summary>
        /// <param name="variableSource">The source of variables to resolve</param>
        void ResolveVariables(Dictionary<string, object> variableSource);
    }
    
    public class VariableResolverService: IVariableResolverService
    {
        private readonly ILogger _logger;
        private static readonly Regex VarMatchingRegex = new(@"(?<!{){(\w|\.|-)+}");

        public VariableResolverService(ILogger<VariableResolverService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc cref="IVariableResolverService"/>
        public object Resolve(Type t, Dictionary<string, object> variableSource)
        {
            Debug.Assert(t.GetInterface(nameof(IDictable)) is not null, $"Type {t.Name} must be IDictable to be resolved!");

            ResolveVariables(variableSource);

            return ObjectFactory.Create(t, variableSource);
        }

        public void ResolveVariables(Dictionary<string, object> variableSource)
        {
            foreach (var (key, value) in variableSource)
            {
                if (value is not string valueString)
                    continue;

                var variablesToReplace = VarMatchingRegex.Matches(valueString)
                                                         .Select(s => s.Value)
                                                         .ToArray();

                _logger.LogDebug("Replacing {variableCount} variables on key {key}", variablesToReplace.Length, key);

                foreach (var varToReplace in variablesToReplace)
                {
                    var sanitizedVarName = SanitizeVariableName(varToReplace);
                    if (!variableSource.TryGetValue(sanitizedVarName, out var replacement))
                        throw new InvalidOperationException($"Unable to locate variable {varToReplace} in variables sources.");

                    _logger.LogDebug("Replacing {key} with {value}", varToReplace, replacement);

                    valueString = valueString.Replace(varToReplace, replacement.ToString());
                    variableSource[key] = valueString;
                }
            }
        }

        private static string SanitizeVariableName(string name) => name.Trim('{', '}');
    }
}