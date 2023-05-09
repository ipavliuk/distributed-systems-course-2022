using ReplicatedLog.Master.Services.Options;

namespace ReplicatedLog.Master.Services.Utils;

public class RetryWithExponentialBackoff
{
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> func, RetryOptions retryPolicies, ILogger logger, Func<Task> handleFailure)
    {
        int maxRetries = retryPolicies.MaxRetries;
        int baseDelayMs = retryPolicies.BaseDelayMs;
        int retries = 0;
        int delayMs = baseDelayMs;

        while (retries < maxRetries)
        {
            try
            {
                // Call the function
                return await func();
            }
            catch (Exception ex) when (retries < maxRetries)
            {
                // Log the exception
                logger.LogWarning($"Retry {retries + 1}: {ex.Message}");

                // Calculate the delay before the next retry
                delayMs = (int)Math.Pow(2, retries) * baseDelayMs;
                retries++;

                // Wait before retrying the operation
                await Task.Delay(delayMs);
            }
            catch (Exception ex)
            {
                // Log the final exception and re-throw it
                logger.LogError($"Final retry error: {ex.Message}");
                await handleFailure();
                return default;
                //throw;
            }
        }
        return default;
    }
}
