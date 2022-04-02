/*
 * Shamelessly stolen from https://github.com/adamreeve/semver.net,
 * the author of which having decided that it be better for consumers to reimplement this
 * functionality rather than make it available.
 */

using System;
using System.Text.RegularExpressions;
using Version = SemanticVersioning.Version;
using System.Linq;

namespace Husky.Internal.Shared
{
    // A version that might not have a minor or patch
    // number, for use in ranges like "^1.2" or "2.x"
    internal record PartialVersion
    {
        public int? Major { get; }

        public int? Minor { get; }

        public int? Patch { get; }

        public string? PreRelease { get; }

        private static Regex regex = new(@"^
                [v=\s]*
                (\d+|[Xx\*])                      # major version
                (
                    \.
                    (\d+|[Xx\*])                  # minor version
                    (
                        \.
                        (\d+|[Xx\*])              # patch version
                        (\-?([0-9A-Za-z\-\.]+))?  # pre-release version
                        (\+([0-9A-Za-z\-\.]+))?   # build version (ignored)
                    )?
                )?
                $",
            RegexOptions.IgnorePatternWhitespace);

        public PartialVersion(int? major, int? minor = null, int? patch = null, string? preRelease = null)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            PreRelease = preRelease;
        }

        public PartialVersion(string input)
        {
            string[] xValues = { "X", "x", "*" };

            if (input.Trim() == "")
            {
                // Empty input means any version
                return;
            }

            var match = regex.Match(input);
            if (!match.Success)
            {
                throw new ArgumentException(string.Format("Invalid version string: \"{0}\"", input));
            }

            if (xValues.Contains(match.Groups[1].Value))
            {
                Major = null;
            }
            else
            {
                Major = int.Parse(match.Groups[1].Value);
            }

            if (match.Groups[2].Success)
            {
                if (xValues.Contains(match.Groups[3].Value))
                {
                    Minor = null;
                }
                else
                {
                    Minor = int.Parse(match.Groups[3].Value);
                }
            }

            if (match.Groups[4].Success)
            {
                if (xValues.Contains(match.Groups[5].Value))
                {
                    Patch = null;
                }
                else
                {
                    Patch = int.Parse(match.Groups[5].Value);
                }
            }

            if (match.Groups[6].Success)
            {
                PreRelease = match.Groups[7].Value;
            }
        }

        public Version ToZeroVersion() => new(Major ?? 0, Minor ?? 0, Patch ?? 0, PreRelease);
    }
}
