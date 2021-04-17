using System;
using System.Collections.Generic;

namespace Husky.Core
{
    public static class HuskyVariables
    {
        /*
         * Todo: Use a source generate to pick up the static hierarchy of variables here and automagically construct the dictionary.
         * Once done, make the class sealed/partial and add a private constructor.
         */
        public static IReadOnlyDictionary<string, object> AsDictionary()
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                [$"{nameof(Folders)}.{nameof(Folders.ProgramFiles)}"] = Folders.ProgramFiles,
                [$"{nameof(Folders)}.{nameof(Folders.Desktop)}"] = Folders.Desktop
            };
        }

        /*
         * Todo: We're going to need to modify this for x-plat purposes. Going to need to reconcile the differences where
         *       something like "ProgramFiles" makes sense on Windows machines, but Linux systems don't have that same notion.
         */
        public static class Folders
        {
            public static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }
}
