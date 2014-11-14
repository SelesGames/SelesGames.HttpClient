
namespace SelesGames.HttpClient.RetryPolicies
{
    public interface IRetryPolicy
    {
        // Summary:
        //     Determines whether the operation should be retried and the interval until
        //     the next retry.
        //
        // Parameters:
        //   retryContext:
        //     A Microsoft.WindowsAzure.Storage.RetryPolicies.RetryContext object that indicates
        //     the number of retries, the results of the last request, and whether the next
        //     retry should happen in the primary or secondary location, and specifies the
        //     location mode.
        //
        // Returns:
        //     A Microsoft.WindowsAzure.Storage.RetryPolicies.RetryInfo object that indicates
        //     the location mode, and whether the next retry should happen in the primary
        //     or secondary location. If null, the operation will not be retried.
        RetryInfo Evaluate(RetryContext retryContext);
    }
}
