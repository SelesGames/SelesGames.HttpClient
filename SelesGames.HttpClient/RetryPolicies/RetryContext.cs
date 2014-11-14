using System.Net.Http;

namespace SelesGames.HttpClient
{
    public class RetryContext
    {
        public int CurrentRetryCount { get; internal set;  }
        public HttpResponseMessage LastRequestResult { get; internal set; }
    }
}