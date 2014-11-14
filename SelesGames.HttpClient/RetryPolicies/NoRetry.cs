
namespace SelesGames.HttpClient.RetryPolicies
{
    public class NoRetry : IRetryPolicy
    {
        public RetryInfo Evaluate(RetryContext retryContext)
        {
            return new RetryInfo(false);
        }
    }
}