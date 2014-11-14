using System;

namespace SelesGames.HttpClient.RetryPolicies
{
    public class LinearRetry : IRetryPolicy
    {
        TimeSpan deltaBackoff;
        int maxAttempts;

        public LinearRetry(TimeSpan deltaBackoff, int maxAttempts)
        {
            this.deltaBackoff = deltaBackoff;
            this.maxAttempts = maxAttempts;
        }

        public RetryInfo Evaluate(RetryContext retryContext)
        {
            if (retryContext.CurrentRetryCount < maxAttempts)
                return new RetryInfo(true) { RetryInterval = deltaBackoff };
            else
                return new RetryInfo(false);
        }
    }
}