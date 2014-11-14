using System;

namespace SelesGames.HttpClient.RetryPolicies
{
    public class RetryInfo
    {
        public RetryInfo() { }
        public RetryInfo(bool shouldRetry)
        {
            ShouldRetry = shouldRetry;
        }

        //public RetryInfo();
        //public RetryInfo(RetryContext retryContext);

        public bool ShouldRetry { get; set; }

        // Summary:
        //     Gets the interval until the next retry.
        public TimeSpan RetryInterval { get; set; }
    }
}
