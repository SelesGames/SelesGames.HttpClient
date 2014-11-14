using System;
using System.Net.Http;

namespace SelesGames.HttpClient
{
    public class ErrorResponseException : Exception
    {
        public HttpResponseMessage ResponseMessage { get; protected set; }

        public ErrorResponseException(HttpResponseMessage response)
        {
            ResponseMessage = response;
        }

        public ErrorResponseException(HttpResponseMessage response, string message)
            : base(message)
        {
            ResponseMessage = response;
        }

        public ErrorResponseException(HttpResponseMessage response, string message, Exception innerException)
            : base(message, innerException)
        {
            ResponseMessage = response;
        }
    }
}
