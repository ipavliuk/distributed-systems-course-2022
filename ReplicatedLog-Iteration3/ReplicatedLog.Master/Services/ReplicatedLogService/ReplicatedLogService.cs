using Common.Model;
using Common.Repository;
using Microsoft.Extensions.Options;
using ReplicatedLog.Common.Exceptions;
using ReplicatedLog.Common.ReplicationBacklog;
using ReplicatedLog.Master.HeartBeat;
using ReplicatedLog.Master.Services.Options;
using ReplicatedLog.Master.Services.Utils;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ReplicatedLog.Master.Services.ReplicatedLogService
{
    public class ReplicatedLogService : IReplicatedLogService
    {
        private readonly ILogger _logger;
        private readonly IRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IClusterHealthManager _healthManager;
        private readonly IOptions<RetryOptions> _retryOptions;
        private readonly IReplicationBacklog _replicationBacklog;

        public ReplicatedLogService(IRepository repository,
                                    IHttpClientFactory httpClientFactory,
                                    IConfiguration configuration,
                                    ILogger<ReplicatedLogService> logger,
                                    IClusterHealthManager healthManager,
                                    IOptions<RetryOptions> retryOptions,
                                    IReplicationBacklog replicationBacklog)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _healthManager = healthManager;
            _retryOptions = retryOptions;
            _replicationBacklog = replicationBacklog;
        }


        public async Task AppendMessageToLog(string message, int writeConcern)
        {
            long id = IdProvider.GenerateId();
            var msg = new Message(id, message);

            var secondaryUrls = _configuration.GetSection("Secondaries:Urls").Get<List<string>>();

            var tasks = new List<Task>();
            var latch = new CountDownLatch(writeConcern - 1); // considering master node

            _logger.LogInformation("Master append message to Log {message.Id}", msg.SequenceId);
            _repository.Add(msg);


            foreach (var secondaryUrl in secondaryUrls)
            {
                _logger.LogInformation("Master start replicating log to {secondaryUrl}", secondaryUrl);

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!_healthManager.IsNodeAvailable(secondaryUrl))
                {
                    _logger.LogWarning("Secondary {secondaryUrl} is not available. Adding message {msg.SequenceId} to replication backlog.", secondaryUrl, msg.SequenceId);
                    _replicationBacklog.AddMessageToBacklog(secondaryUrl, msg, latch);
                    continue;
                }

                try
                {
                    //The List<Task> called tasks is used to hold all the tasks that are started to replicate the message to the secondary nodes.
                    //This allows the method to asynchronously wait for all the tasks to complete, which is necessary to ensure that the required number of acknowledgments have been received before the method can complete.
                    //Each task executes the RetryWithExponentialBackoff.ExecuteAsync() method, which attempts to send the message to the secondary node and retries with exponential backoff in case of failure.
                    //Once a task completes, the CountDownLatch object is decremented to indicate that the task has finished.
                    tasks.Add(RetryWithExponentialBackoff.ExecuteAsync(async () =>
                    {
                        using (var content = new StringContent(JsonSerializer.Serialize(msg)))
                        {
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            using (var request = new HttpRequestMessage(HttpMethod.Post, $"{secondaryUrl}/api/log") { Content = content })
                            {
                                var result = await httpClient.SendAsync(request);
                                result.EnsureSuccessStatusCode();
                                _logger.LogInformation("Replication to {secondaryUrl} completed successfully", secondaryUrl);
                                latch.CountDown(); // decrement the latch on success
                            }
                        }
                        return Task.CompletedTask; // return a completed task object
                    },
                    _retryOptions.Value,
                    _logger,
                    async () =>
                    {
                        _logger.LogError($"Retry attempts exceeded. Adding message {msg.SequenceId} to backlog for secondary server {secondaryUrl}");
                        _replicationBacklog.AddMessageToBacklog(secondaryUrl, msg, latch);
                    }));
                }
                catch (HttpRequestException ex)//No need this
                {
                    _logger.LogError("Error calling service {secondaryUrl}", secondaryUrl);
                    throw new ConnectionFailureException("Failed to connect to Secondary server.");
                }
            }

            if (writeConcern > 1)
            {
                await latch.WaitAsync(); // wait for the required number of acknowledgements
            }
        }

        public List<Message> GetAllMessages()
        {
            return _repository.GetAll();
        }

    }
}
