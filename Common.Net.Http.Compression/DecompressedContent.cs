using Common.Net.Http.Compression.Settings;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    public class DecompressedContent : HttpContent
    {
        HttpContent originalContent;
        CompressionHandler compressionHandler;

        internal DecompressedContent(
            HttpContent originalContent,
            CompressionHandler compressionHandler)
        {
            if (originalContent == null) throw new ArgumentNullException("content");
            if (compressionHandler == null) throw new ArgumentNullException("handler");

            this.originalContent = originalContent;
            this.compressionHandler = compressionHandler;

            CopyHeaders(originalContent, this);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (var ogStream = await originalContent.ReadAsStreamAsync().ConfigureAwait(false))
            using (var gzip = compressionHandler.Decompress(ogStream))
            {
                await gzip.CopyToAsync(stream).ConfigureAwait(false);
                gzip.Flush();
            }
        }

        static void CopyHeaders(HttpContent source, HttpContent destination)
        {
            // copy the headers from the original content
            foreach (var header in source.Headers)
            {
                destination.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }




        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                return;

            originalContent.Dispose();
        }

        #endregion
    }
}