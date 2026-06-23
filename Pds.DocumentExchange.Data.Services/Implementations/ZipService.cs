using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// The zip compression service.
    /// </summary>
    public class ZipService : IZipService
    {
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when the files collection is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the collection contains empty file names or contents.</exception>
        public byte[] ZipFiles(IEnumerable<(string Name, byte[] Content)> files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files), "The files collection cannot be null.");
            }

            if (files.Any(file => string.IsNullOrWhiteSpace(file.Name)))
            {
                throw new ArgumentException("There cannot be empty file names.", nameof(files));
            }

            if (files.Any(file => file.Content == null || file.Content.Length == 0))
            {
                throw new ArgumentException("There cannot be empty file contents.", nameof(files));
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                {
                    foreach (var (name, content) in files)
                    {
                        var entry = zipArchive.CreateEntry(name, CompressionLevel.Fastest);
                        using (var writer = new BinaryWriter(entry.Open()))
                        {
                            writer.Write(content);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}