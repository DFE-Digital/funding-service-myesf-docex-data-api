using Pds.DocumentExchange.Data.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pds.DocumentExchange.Data.Repository.Implementations
{
    /// <summary>The FileRepository class - provides methods for processing files included in the current assembly.</summary>
    /// <seealso cref="IFileRepository" />
    public class FileRepository : IFileRepository
    {
        /// <summary>Reads the file contents.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The file contents as a string.</returns>
        public string ReadSingleFileContents(string fileName)
        {
            string fileContents = null;

            var assembliesContainingResources = GetAssembliesContainingResources();

            foreach (var pair in assembliesContainingResources)
            {
                var fileBodyPath = pair.Value.FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(fileBodyPath))
                {
                    fileContents = ReadFileContents(pair.Key, fileBodyPath);
                    break;
                }
            }

            return fileContents;
        }

        /// <summary>Reads multiple files matching a pattern.</summary>
        /// <param name="fileNamePattern">The file name pattern.</param>
        /// <returns>A dictionary containing a key value pair of filename and its contents.</returns>
        public Dictionary<string, string> ReadMultipleFilesMatchingAPattern(string fileNamePattern)
        {
            var fileNamesAndContents = new Dictionary<string, string>();

            var assembliesContainingResources = GetAssembliesContainingResources();

            foreach (var pair in assembliesContainingResources)
            {
                var fileNames = pair.Value.Where(r => r.EndsWith(fileNamePattern, StringComparison.OrdinalIgnoreCase));
                foreach (var resourceName in fileNames.Where(resourceName => !string.IsNullOrEmpty(resourceName)))
                {
                    var fileName = GetFileNameFromResourceName(resourceName);
                    var fileContents = ReadFileContents(pair.Key, resourceName);
                    if (!string.IsNullOrWhiteSpace(fileContents))
                    {
                        fileNamesAndContents.Add(fileName, fileContents);
                    }
                }
            }

            return fileNamesAndContents;
        }

        private static string GetFileNameFromResourceName(string resourceName)
        {
            var fileName = Path.GetFileNameWithoutExtension(resourceName);

            if (fileName.Contains('.'))
            {
                fileName = fileName.Substring(1 + fileName.LastIndexOf(".", StringComparison.Ordinal));
            }

            return fileName;
        }

        private static Dictionary<Assembly, string[]> GetAssembliesContainingResources()
        {
            var assembliesContainingResources = new Dictionary<Assembly, string[]>();
            var stackTrace = new StackTrace(); // get call stack
            var stackFrames = stackTrace.GetFrames(); // get method calls (frames)

            foreach (var stackFrame in stackFrames)
            {
                var declaringType = stackFrame.GetMethod().DeclaringType;
                if (declaringType != null)
                {
                    var typeAssembly = declaringType.Assembly;

                    if (!ExcludeFilesFromTheseAssemblies(typeAssembly))
                    {
                        var resourceNames = typeAssembly.GetManifestResourceNames();
                        if (resourceNames.Any() && !assembliesContainingResources.ContainsKey(typeAssembly))
                        {
                            assembliesContainingResources.Add(typeAssembly, resourceNames);
                        }
                    }
                }
            }

            return assembliesContainingResources;
        }

        private static bool ExcludeFilesFromTheseAssemblies(Assembly typeAssembly)
        {
            return new[] { "System.Private.CoreLib", "Swashbuckle.AspNetCore.Swagger", "Microsoft." }
                .Any(name => typeAssembly.FullName.Contains(name));
        }

        private static string ReadFileContents(Assembly typeAssembly, string fileBodyPath)
        {
            string fileContents = null;

            using (var stream = typeAssembly.GetManifestResourceStream(fileBodyPath))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        fileContents = reader.ReadToEnd();
                    }
                }
            }

            return fileContents;
        }
    }
}