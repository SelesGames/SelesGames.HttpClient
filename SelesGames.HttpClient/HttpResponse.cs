using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace SelesGames.HttpClient
{
    public class HttpResponse : IDisposable
    {
        bool isDisposed = false;

        public MediaTypeFormatterCollection Formatters { get; private set; }
        public HttpResponseMessage HttpResponseMessage { get; private set; }

        public HttpResponse(HttpResponseMessage httpResponseMessage, MediaTypeFormatterCollection formatters)
        {
            if (httpResponseMessage == null) throw new ArgumentNullException("httpResponseMessage");
            if (formatters == null) throw new ArgumentNullException("formatters");

            this.HttpResponseMessage = httpResponseMessage;
            this.Formatters = formatters;
        }

        public Task<Stream> ReadStream()
        {
            return HttpResponseMessage.Content.ReadAsStreamAsync();
        }

        public async Task<T> Read<T>()
        {
            try
            {
                var result = await HttpResponseMessage.Content.ReadAsAsync<T>(Formatters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                throw new ErrorResponseException(HttpResponseMessage, "parse error in SmartHttpClient.ReadResponseContentAsync", ex);
            }
        }

        public void EnsureSuccessStatusCode()
        {
            if (!HttpResponseMessage.IsSuccessStatusCode)
                throw new ErrorResponseException(HttpResponseMessage);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            HttpResponseMessage.Dispose();
        }
    }
}
