using System;
using System.IO;

namespace Pds.DocumentExchange.Data.Services.Extensions
{
    /// <summary>
    /// The stream extension class.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Extension method for converting a stream to byte array.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <returns>Converted byte array.</returns>
        public static byte[] ToByteArray(this Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException("Stream does not support seeking");
            }

            stream.Position = 0;
            var buffer = new byte[stream.Length];
            for (var totalBytesCopied = 0; totalBytesCopied < stream.Length;)
            {
                totalBytesCopied += stream.Read(
                    buffer,
                    totalBytesCopied,
                    Convert.ToInt32(stream.Length) - totalBytesCopied);
            }

            return buffer;
        }
    }
}