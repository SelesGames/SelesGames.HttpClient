using Common.Net.Http.Compression;
using Common.Net.Http.Compression.Settings;
using SelesGames.HttpClient.RetryPolicies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.HttpClient
{
    /// <summary>
    /// A version of HttpClient that reads the Content-Type header to deserialize a web request response into an object.  
    /// It also auto-decompresses responses.
    /// 
    /// By default, requests compressed Json objects
    /// </summary>
    public class SmartHttpClient
    {
        readonly ContentEncoderSettings encoderSettings;
        readonly CompressionSettings compressionSettings;
        readonly IEnumerable<string> acceptEncodings;

        public MediaTypeFormatterCollection Formatters { get; private set; }
        public IRetryPolicy RetryPolicy { get; private set; }
        public TimeSpan Timeout { get; set; }

        // 100 seconds matches the default timeout of the inner HttpClient
        static TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(100);


        #region Constructors

        public SmartHttpClient(CompressionSettings compressionSettings = CompressionSettings.AcceptEncoding | CompressionSettings.ContentEncoding)
            : this(ContentEncoderSettings.Default, compressionSettings) { }

        public SmartHttpClient(
            ContentEncoderSettings encoderSettings,
            CompressionSettings compressionSettings = CompressionSettings.AcceptEncoding | CompressionSettings.ContentEncoding)
        {
            this.encoderSettings = encoderSettings;
            this.compressionSettings = compressionSettings;
            this.Formatters = new StandardMediaTypeFormatters();
            this.RetryPolicy = Retry.None;
            this.Timeout = DEFAULT_TIMEOUT;

            if (GlobalCompressionSettings.CompressionHandlers == null)
            {
                this.acceptEncodings = new List<string>();
                System.Diagnostics.Debug.WriteLine("\r\n\r\nNO GLOBAL COMPRESSION HANDLERS have been set.  If you want to enable compression/decompression of HTTP requests, you have to set Common.Compression.Settings to a valid compression handler collection.\r\n******************************************\r\n");
            }
            else
            {
                this.acceptEncodings = GlobalCompressionSettings.CompressionHandlers.GetSupportedEncodings().ToArray();
            }
        }

        #endregion




        #region Send Async (send any HttpRequestMessage)

        public async Task<HttpResponse> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            try
            {
                var response = await CreateClient()
                    .SendAsync(request, cancellationToken)
                    .ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                return new HttpResponse(response, this.Formatters);
            }
            catch(TaskCanceledException)
            {
                var actualElapsed = DateTime.UtcNow - now;
                var requestTimeoutException = new RequestTimeoutException(request, Timeout, actualElapsed);
                throw requestTimeoutException;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
        }

        #endregion




        #region GET

        public Task<HttpResponse> GetAsync(string url, CancellationToken cancellationToken)
        {
            var request = CreateRequest(HttpMethod.Get, url);
            return SendAsync(request, cancellationToken);
        }

        public Task<HttpResponse> GetAsync(string url)
        {
            return GetAsync(url, CancellationToken.None);
        }

        public Task<T> GetAsync<T>(string url)
        {
            return GetAsync<T>(url, CancellationToken.None);
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            using (var response = await GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Read<T>().ConfigureAwait(false);
                return result;
            }
        }

        #endregion




        #region POST

        public Task<HttpResponse> PostAsync<T>(string url, T obj, CancellationToken cancellationToken)
        {
            var request = CreateRequest(HttpMethod.Post, url, obj);
            return SendAsync(request, cancellationToken);
        }

        public Task<HttpResponse> PostAsync<T>(string url, T obj)
        {
            return PostAsync(url, obj, CancellationToken.None);
        }

        public Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj)
        {
            return PostAsync<TPost, TResult>(url, obj, CancellationToken.None);
        }

        public async Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj, CancellationToken cancellationToken)
        {
            using (var response = await PostAsync(url, obj, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Read<TResult>().ConfigureAwait(false);
                return result;
            }
        }

        #endregion




        #region Create the HttpClient

        System.Net.Http.HttpClient CreateClient()
        {
            //var client = new System.Net.Http.HttpClient(new RetryHandler(new HttpClientCompressionHandler(), RetryPolicy ?? Retry.None));
            var client = new System.Net.Http.HttpClient(new HttpClientCompressionHandler());
            client.Timeout = Timeout;
            return client;
        }

        #endregion




        #region Create the HttpRequestMessage based on settings

        HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            var accept = encoderSettings.Accept;
            if (!string.IsNullOrEmpty(accept))
                request.Headers.TryAddWithoutValidation("Accept", accept);

            if (compressionSettings.HasFlag(CompressionSettings.AcceptEncoding) && acceptEncodings.Any())
                request.Headers.TryAddWithoutValidation("Accept-Encoding", acceptEncodings);

            return request;
        }

        HttpRequestMessage CreateRequest<T>(HttpMethod method, string url, T obj)
        {
            var request = CreateRequest(method, url);

            var mediaType = new MediaTypeHeaderValue(encoderSettings.ContentType);
            var formatter = Formatters.FindWriteFormatter<T>(mediaType);

            var content = new ObjectContent<T>(obj, formatter, mediaType);

            if (compressionSettings.HasFlag(CompressionSettings.ContentEncoding))
                content.Headers.TryAddWithoutValidation("Content-Encoding", "gzip");

            request.Content = content;
            return request;
        }

        #endregion
    }
}