using Common.Net.Http.Compression.Settings;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    public class HttpClientCompressionHandler : HttpClientHandler
    {
        public override bool SupportsAutomaticDecompression
        {
            get { return false; }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request = CompressRequest(request);

            return base
                .SendAsync(request, cancellationToken)
                .ContinueWith(o => DecompressResponse(o.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        static HttpRequestMessage CompressRequest(HttpRequestMessage request)
        {
            var content = request.Content;
            if (content != null)
            {
                var contentEncoding = content.Headers.ContentEncoding;
                if (contentEncoding != null && contentEncoding.Any())
                {
                    string encodingType = contentEncoding.First() ?? "";
                    var compressionHandler = GlobalCompressionSettings.CompressionHandlers.Find(encodingType);
                    if (compressionHandler == null)
                        throw new Exception("no compression handler was found for encoding type: " + encodingType);

                    request.Content = new CompressedContent(content, compressionHandler);
                }
            }
            return request;
        }

        static HttpResponseMessage DecompressResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var contentEncoding = response.Content.Headers.ContentEncoding;
                if (contentEncoding != null && contentEncoding.Any())
                {
                    string encodingType = contentEncoding.First() ?? "";
                    var compressionHandler = GlobalCompressionSettings.CompressionHandlers.Find(encodingType);
                    if (compressionHandler == null)
                        throw new Exception("no compression handler was found for encoding type: " + encodingType);

                    response.Content = new DecompressedContent(response.Content, compressionHandler);
                }
            }

            return response;
        }
    }
}