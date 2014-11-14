using System;
using System.Net.Http;

namespace SelesGames.HttpClient
{
    public class RequestTimeoutException : Exception
    {
        public HttpRequestMessage Request { get; private set; }
        public TimeSpan RequestTimeout { get; private set; }
        public TimeSpan ActualElapsed { get; private set; }

        public RequestTimeoutException(HttpRequestMessage request, TimeSpan requestTimeout, TimeSpan actualElapsed)
        {
            Request = request;
            RequestTimeout = requestTimeout;
            ActualElapsed = actualElapsed;
        }

        public override string ToString()
        {
            return string.Format("RequestTimeoutException: {0}", Request.RequestUri);
        }
    }
}