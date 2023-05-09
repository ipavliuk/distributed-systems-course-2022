using Microsoft.Extensions.Options;
using ReplicatedLog.Common.ReplicationBacklog;
using ReplicatedLog.Master.Services.Options;
using ReplicatedLog.Master.Services.Utils;
using System.Net.Http.Headers;
using System.Text.Json;


namespace ReplicatedLog.Master.Services.MissedMessageReplicator
{
    public class MissedMessageReplicator : IMissedMessageReplicator
    {
        private readonly IReplicationBacklog _replicationBacklog;
        private readonly ILogger _logger;
        private readonly IOptions<RetryOptions> _retryOptions;
        public MissedMessageReplicator(IReplicationBacklog replicationBacklog,
                                        ILogger<MissedMessageReplicator> logger,
                                        IOptions<RetryOptions> retryOptions)
        {
            _replicationBacklog = replicationBacklog;
            _logger = logger;
            _retryOptions = retryOptions;
        }

        public async Task ReplicateMissedMessagesAsync(string secondaryUrl)
        {
            if (!_replicationBacklog.TryGetMassages(secondaryUrl, out var backlogForSecondary))
            {
                return;
            }

            while (backlogForSecondary.Count > 0)
            {
                var message = backlogForSecondary.Peek();
                try
                {
                    await RetryWithExponentialBackoff.ExecuteAsync<Task>(async () =>
                    {
                        using (var httpClient = new HttpClient())
                        {
                            using (var content = new StringContent(JsonSerializer.Serialize(message.Msg)))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                using (var request = new HttpRequestMessage(HttpMethod.Post, $"{secondaryUrl}/api/log") { Content = content })
                                {
                                    var result = await httpClient.SendAsync(request);
                                    result.EnsureSuccessStatusCode();
                                    _logger.LogInformation("Replication to {secondaryUrl} completed successfully", secondaryUrl);
                                    backlogForSecondary.Dequeue();
                                    message.CountDownLatch.CountDown();
                                }
                            }
                        }
                        return Task.CompletedTask; // return a completed task object
                    },
                    _retryOptions.Value,
                    _logger,
                    () =>
                    {
                        _logger.LogError($"Replicate Retry attempts exceeded. Adding message {message.Msg.SequenceId} to backlog for secondary server {secondaryUrl}");
                        return Task.CompletedTask; // return a completed task object
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to replicate message {message.Msg.SequenceId} to {secondaryUrl}. Adding to backlog again. {ex.Message}");
                    break;
                }
            }

        }
    }
}
