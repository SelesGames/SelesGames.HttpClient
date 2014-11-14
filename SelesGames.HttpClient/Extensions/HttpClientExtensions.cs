using System;
using System.Net.Http;

namespace SelesGames.HttpClient
{
    public static class HttpClientExtensions
    {
        public static IDisposable PollChangesToResource(this SmartHttpClient client, string resourceUrl, TimeSpan pollingInterval, IObserver<HttpResponse> observer)
        {
            var listener = new HttpResourceListener(client, resourceUrl, observer.OnNext, observer.OnError);
            listener.PollingInterval = pollingInterval;
            listener.BeginListening();

            return listener;
        }

        public static IDisposable PollChangesToResource(this SmartHttpClient client,
            string resourceUrl,
            TimeSpan pollingInterval,
            Action<HttpResponse> onUpdated,
            Action<Exception> onException = null)
        {
            var listener = new HttpResourceListener(client, resourceUrl, onUpdated, onException);
            listener.PollingInterval = pollingInterval;
            listener.BeginListening();

            return listener;
        }
    }
}
