using Common.Compression;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    static class HttpContentExtensions
    {
        public static async Task<HttpContent> AsByteArray(
            this HttpContent content,
            CompressionHandler handler,
            Mode mode)
        {
            //var compressionHandler = Common.Compression.Settings.CompressionHandlers.Find(encodingType);
            //if (compressionHandler == null)
            //    throw new Exception("no compression handler was found for encoding type: " + encodingType);

            byte[] decompressedBytes;
            var strategy = SelectStrategy(handler, mode);

            using (var ms = new MemoryStream())
            using (var decompressionStream = strategy(ms))
            {
                await content.CopyToAsync(decompressionStream);
                await decompressionStream.FlushAsync();
                decompressedBytes = ms.ToArray();
            }

            var byteArrayContent = new ByteArrayContent(decompressedBytes);
            CopyHeaders(content, byteArrayContent);
            return byteArrayContent;
        }

        static Func<Stream, Stream> SelectStrategy(CompressionHandler handler, Mode mode)
        {
            if (mode == Mode.Compress)
                return handler.Compress;

            else if (mode == Mode.Decompress)
                return handler.Decompress;

            throw new Exception("unrecognized Mode in SelectStrategy call");
        }

        static void CopyHeaders(HttpContent source, HttpContent destination)
        {
            // copy the headers from the original content
            foreach (var header in source.Headers)
            {
                destination.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}