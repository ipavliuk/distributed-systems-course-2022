namespace ReplicatedLog.Master.Services;

public class RetryOptions
{
    public int MaxRetries { get; set; } = 5;
    public int BaseDelayMs { get; set; } = 500;
}
