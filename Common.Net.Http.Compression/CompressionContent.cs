using Common.Compression;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    public class CompressionContent : HttpContent
    {
        HttpContent originalContent;
        CompressionHandler compressionHandler;
        Mode mode;

        internal CompressionContent(
            HttpContent originalContent,
            CompressionHandler compressionHandler,
            Mode mode)
        {
            if (originalContent == null) throw new ArgumentNullException("content");
            if (compressionHandler == null) throw new ArgumentNullException("handler");

            this.originalContent = originalContent;
            this.compressionHandler = compressionHandler;
            this.mode = mode;

            CopyHeaders(originalContent, this);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var compressionStrategy = SelectCompressionStrategy(compressionHandler, mode);

            // THIS METHOD ONLY WORKS IF THE GZIP STREAMS DON'T CLOSE THEIR UNDERLYING STREAM UPON DISPOSAL 
            //using (var compressionStream = compressionStrategy(stream))
            //using (var contentStream = await originalContent.ReadAsStreamAsync())
            //{
            //    await contentStream.CopyToAsync(compressionStream);
            //}

            // the below method works, but makes an extra copy of the memorystream's byte array buffer so is inefficient

            try
            {
                byte[] readBytes = await originalContent.ReadAsByteArrayAsync();
                byte[] writeBytes;

                // NEW WAY OF COMPRESSING, SEE IF THIS WORKS
                var ms = new MemoryStream(readBytes);

                using (var gzip = compressionStrategy(ms))
                //using (var readStream = await originalContent.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    //await gzip.WriteAsync(readBytes, 0, readBytes.Length);
                    //await readStream.CopyToAsync(gzip).ConfigureAwait(false);
                }
                writeBytes = ms.ToArray();

                await stream.WriteAsync(writeBytes, 0, writeBytes.Length);



                //byte[] bytes;

                //// NEW WAY OF COMPRESSING, SEE IF THIS WORKS
                //var ms = new MemoryStream();

                //using (var gzip = compressionStrategy(ms))
                //using (var readStream = await originalContent.ReadAsStreamAsync().ConfigureAwait(false))
                //{
                //    await readStream.CopyToAsync(gzip).ConfigureAwait(false);
                //}
                //bytes = ms.ToArray();

                //await stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
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

        static Func<Stream, Stream> SelectCompressionStrategy(CompressionHandler handler, Mode mode)
        {
            if (mode == Mode.Compress)
                return handler.Compress;

            else if (mode == Mode.Decompress)
                return handler.Decompress;

            throw new Exception("unrecognized Mode in SelectStrategy call");
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