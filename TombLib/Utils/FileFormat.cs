using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Utils
{
    public struct FileFormat
    {
        public string Description { get; }
        public IReadOnlyList<string> Extensions { get; }

        public FileFormat(string description, params string[] extensions)
        {
            if (extensions.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(extensions));
            Description = description;
            Extensions = extensions;
        }
    }

    public static class FileFormatFunctions
    {
        public static string GetFilter(this IEnumerable<FileFormat> fileFormats)
        {
            // Early exit if there are no extensions
            const string allFiles = "All files (*.*)|*.*";
            if (fileFormats.FirstOrDefault().Description == null)
                return allFiles;

            // Enumerate all supported formats
            string result = "Any supported format|";
            foreach (FileFormat fileFormat in fileFormats)
                foreach (string extension in fileFormat.Extensions)
                    result += "*." + extension + ";";
            result = result.Substring(0, result.Length - 1);

            // Add every format type separately
            foreach (FileFormat fileFormat in fileFormats)
            {
                result += "|" + fileFormat.Description + " (";
                foreach (string extension in fileFormat.Extensions)
                    result += "*." + extension + ", ";
                result = result.Substring(0, result.Length - 2) + ")";

                result += "|";
                foreach (string extension in fileFormat.Extensions)
                    result += "*." + extension + ";";
                result = result.Substring(0, result.Length - 1);
            }

            // Add all files
            result += "|" + allFiles;
            return result;
        }

        public static bool Matches(this IEnumerable<FileFormat> fileFormats, string fileExtension)
        {
            foreach (FileFormat fileFormat in fileFormats)
                foreach (string extension in fileFormat.Extensions)
                    if (fileExtension.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
                        return true;
            return false;
        }
    }
}
