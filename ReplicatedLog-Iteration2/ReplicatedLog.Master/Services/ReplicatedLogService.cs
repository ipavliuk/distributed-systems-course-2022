using Common.Model;
using Common.Repository;
using ReplicatedLog.Common.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ReplicatedLog.Master.Services
{
    public class ReplicatedLogService : IReplicatedLogService
    {
        private readonly ILogger _logger;
        private readonly IRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ReplicatedLogService(IRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ReplicatedLogService> logger)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
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

                try
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
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
                            
                        }
                        catch (HttpRequestException ex)
                        {
                            _logger.LogError("Error calling service {secondaryUrl}", secondaryUrl);
                        }
                    }));
                }
                catch (HttpRequestException ex)
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
