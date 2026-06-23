using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Storage
{
    /// <summary>
    /// The local file helper.
    /// </summary>
    public class LocalFileHelper
    {
        private static byte[] _defaultDocument;
        private static ConcurrentDictionary<string, byte[]> _documents = new ConcurrentDictionary<string, byte[]>();

        /// <summary>
        /// Loads the example documents.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public static async Task LoadExampleDocuments()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames().Where(str => str.EndsWith(".docx") && str.Contains("Example_"));

            foreach (var currentResourceName in resourceNames)
            {
                var document = await GetDocumentResourceByName(assembly, currentResourceName);

                var split = Path.GetFileNameWithoutExtension(currentResourceName).Split('_');
                _documents.TryAdd(split[1], document);
            }

            _defaultDocument = await GetExampleDocumentBytes();
        }

        /// <summary>
        /// Gets the example document content.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/> returning the document content.</returns>
        public static async Task<byte[]> GetExampleDocumentBytes()
        {
            // NOTE: Not sure if this would work if invoked from outside of this project...
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("Assets.Example.docx"));

            return await GetDocumentResourceByName(assembly, resourceName);
        }

        /// <summary>
        /// Gets the example document by size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The document content.</returns>
        public static byte[] GetExampleDocumentBySize(string size)
           => _documents.GetValueOrDefault(size, _defaultDocument);

        private static async Task<byte[]> GetDocumentResourceByName(Assembly assembly, string resourceName)
        {
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            using var memoryStream = new MemoryStream();

            await resourceStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}