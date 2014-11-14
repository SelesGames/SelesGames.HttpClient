using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.HttpClient
{
    public static class LogSettings
    {
        public static bool IsLoggingEnabled { get; set; }
    }

    public class LogHandler : DelegatingHandler
    {
        public Action<string> Log { get; set; }

        public LogHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
#if DEBUG
            if (LogSettings.IsLoggingEnabled)
                Log = s => System.Diagnostics.Debug.WriteLine(s);
#endif
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await TraceRequest(request).ConfigureAwait(false);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            await TraceResponse(response).ConfigureAwait(false);

            return response;
        }




        #region Trace the request and response using the specificied Log delegate

        async Task TraceRequest(HttpRequestMessage request)
        {
            if (Log == null)
                return;

            string output;
            var contentString = await ReadContentAsString(request.Content);

            if (string.IsNullOrEmpty(contentString))
            {
                output = string.Format(
                    "Sending request...)\r\n{0}\r\n\r\n",
                    request.ToString());
            }
            else
            {
                output = string.Format(
                    "Sending request...)\r\n{0}\r\nCONTENT:\r\n{1}\r\n\r\n",
                    request.ToString(),
                    contentString);
            }

            Log(output);
        }

        async Task<string> ReadContentAsString(HttpContent content)
        {
            if (!(content is ObjectContent))
                return null;

            var objectContent = (ObjectContent)content;
            var copy = new ObjectContent(objectContent.ObjectType, objectContent.Value, objectContent.Formatter);

            return await copy.ReadAsStringAsync();
        }

        async Task TraceResponse(HttpResponseMessage response)
        {
            if (Log == null)
                return;

            string output;

            var responseString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseString))
            {
                output = string.Format(
                    "Received response ... \r\n{0}\r\n\r\n",
                    response.ToString());
            }
            else
            {
                output = string.Format(
                    "Received response ... \r\n{0}\r\nRESPONSE:\r\n{1}\r\n\r\n",
                    response.ToString(),
                    responseString);
            }

            Log(output);
        }

        #endregion
    }
}