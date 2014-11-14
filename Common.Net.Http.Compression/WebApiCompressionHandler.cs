using Common.Net.Http.Compression.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    /// <summary>
    /// Compresses outgoing HttpResponseMessage Content if the original request 
    /// specifed that it could accept compressed content 
    /// (via Accept-Encoding: gzip, deflate).
    /// 
    /// Decompresses incoming content if the Content-Encoding is gzip or deflate
    /// </summary>
    public class WebApiCompressionHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request = DecompressRequest(request);

            return base.SendAsync(request, cancellationToken)
                .ContinueWith(o => CompressResponse(o.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        static HttpRequestMessage DecompressRequest(HttpRequestMessage request)
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

                    request.Content = new DecompressedContent(content, compressionHandler);
                }
            }
            return request;
        }

        static HttpResponseMessage CompressResponse(HttpResponseMessage response)
        {
            if (response.HasEncodeableContent())
            {
                var acceptEncoding = response.RequestMessage.Headers.AcceptEncoding;

                if (acceptEncoding != null && acceptEncoding.Any())
                {
                    var acceptEncodings = GetFilteredEncodings(acceptEncoding);

                    if (acceptEncodings != null && acceptEncodings.Any())
                    {
                        var handler = GlobalCompressionSettings.CompressionHandlers.Find(acceptEncodings).FirstOrDefault();

                        if (handler != null)
                        {
                            var contentEncoding = handler.SupportedEncodings.First();

                            response.Content.Headers.ContentEncoding.Clear();
                            response.Content.Headers.ContentEncoding.Add(contentEncoding);
                            response.Content = new CompressedContent(response.Content, handler);
                        }
                    }
                }
            }

            return response;
        }

        static IEnumerable<string> GetFilteredEncodings(IEnumerable<StringWithQualityHeaderValue> headerValues)
        {
            foreach (var headerValue in headerValues)
            {
                if (headerValue != null &&
                    headerValue.Value != null &&
                    !headerValue.Value.Equals("identity", StringComparison.OrdinalIgnoreCase))
                {
                    yield return headerValue.Value;
                }
            }
        }
    }
}