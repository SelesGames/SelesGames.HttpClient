using SelesGames.HttpClient.RetryPolicies;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.HttpClient
{
    public class RetryHandler : DelegatingHandler
    {
        IRetryPolicy retryPolicy;

        public RetryHandler(HttpMessageHandler innerHandler, IRetryPolicy retryPolicy)
            : base(innerHandler)
        {
            if (retryPolicy == null) throw new ArgumentNullException("retryPolicy");

            this.retryPolicy = retryPolicy;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAsync(request, null, cancellationToken);
        }




        #region actual SendAsync implementation

        async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, RetryContext retryContext, CancellationToken cancellationToken)
        {
            if (retryContext != null)
            {
                var retryInfo = retryPolicy.Evaluate(retryContext);
                if (!retryInfo.ShouldRetry)
                    throw new ErrorResponseException(retryContext.LastRequestResult);

                await Task.Delay(retryInfo.RetryInterval).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (IsRetryableResponseCode(response.StatusCode))
            {
                retryContext = retryContext ?? new RetryContext();

                retryContext.CurrentRetryCount++;
                retryContext.LastRequestResult = response;

                request = Copy(request);
                return await SendAsync(request, retryContext, cancellationToken);
            }

            else
            {
                return response;
            }
        }

        static bool IsRetryableResponseCode(HttpStatusCode statusCode)
        {
            return 
                statusCode == HttpStatusCode.InternalServerError;
        }

        HttpRequestMessage Copy(HttpRequestMessage original)
        {
            var request = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Content = original.Content,
                Version = original.Version,
            };

            foreach (var header in original.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var property in original.Properties)
            {
                request.Properties.Add(property.Key, property.Value);
            }

            return request;
        }

        #endregion
    }
}