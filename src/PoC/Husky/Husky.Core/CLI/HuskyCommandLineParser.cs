using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Husky.Core.Extensions;
using static Husky.Core.HuskyConstants.StepTags;

namespace Husky.Core.CLI
{
    internal record Option(string Name, string Value)
    {
        public string GetAsString() => Value;
        public int GetAsInt() => int.Parse(Value);
        public bool GetAsBool() => bool.Parse(Value);
    }

    internal class ParseResult
    {
        public string Command { get; }

        private Dictionary<string, Option> OptionsByName { get; } = new();

        public ParseResult(string command, IEnumerable<Option> options)
        {
            Command = command;
            OptionsByName = options.ToDictionary(k => k.Name);
        }

        public void AddOption(string name, string value) => OptionsByName[name] = new Option(name, value);

        public bool TryGetOption(string name, [NotNullWhen(true)] out Option? o) => OptionsByName.TryGetValue(name, out o);
    }


    internal static class HuskyCommandLineParser
    {
        public static readonly (string name, string description)[] AvailableCommands =
        {
            (Install, "Installs this application"),
            (Uninstall, "Uninstall this application"),
            (Modify, "Modifies this application"),
            (Repair, "Repairs this application")
        };

        public static readonly (string name, string description)[] AvailableOptions =
        {
            ("--dry-run", "Executes without modifying the current machine."),
            ("--verbosity", "The verbosity at which to log events within the installation process")
        };

        public static string GetHelpText() => $@"Husky Installer v{Assembly.GetExecutingAssembly().GetName().Version}

The following Operations are supported:

{string.Join(Environment.NewLine, AvailableCommands.Select(s => $"{s.name}: {s.description}"))}

The following Options are supported:

{string.Join(Environment.NewLine, AvailableOptions.Select(s => $"{s.name}: {s.description}"))}
";

        public static ParseResult? Parse(string[] args)
        {
            const string optionsPrefix = "--";

            var verb = args.Where(w => !w.StartsWith(optionsPrefix))
                           .Intersect(AvailableCommands.Select(s => s.name))
                           .ToArray();

            static ParseResult? HandleError(string error)
            {
                Console.Error.WriteLine(error);
                Console.WriteLine(GetHelpText());
                return null;
            }

            var options = args.Where(w => w.StartsWith(optionsPrefix))
                              .Select(s => s.TrimStart('-').Split('='))
                              .Select(s => new Option(s[0], s.Length == 1 ? "True": s[1]))
                              .ToArray();

            var unrecognizedArgs = options.Select(s => s.Name)
                                          .Except(AvailableOptions.Select(s => s.name.TrimStart('-')))
                                          .Concat(args.Where(w => !w.StartsWith(optionsPrefix))
                                                      .Except(AvailableCommands.Select(s => s.name)))
                                          .Where(w => !string.IsNullOrWhiteSpace(w))
                                          .ToArray();

            return verb.Length switch
                   {
                       > 1                                   => HandleError("Multiple commands entered."),
                       _ when unrecognizedArgs.Length > 0 => HandleError($"Unrecognized options entered: {unrecognizedArgs.Csv()}"),
                       1                                     => new ParseResult(verb[0], options),
                       0                                     => new ParseResult(Install, options),
                       _                                     => throw new InvalidOperationException("Literally how?")
                   };
        }
    }
}