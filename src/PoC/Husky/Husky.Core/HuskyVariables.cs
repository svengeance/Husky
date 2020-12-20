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
        public static IReadOnlyDictionary<string, string> AsDictionary()
        {
            return new Dictionary<string, string>
            {
                [$"{nameof(Folders)}.{nameof(Folders.ProgramFiles)}"] = Folders.ProgramFiles
            };
        }

        public static class Folders
        {
            public static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }
    }
}