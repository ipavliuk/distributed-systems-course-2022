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

        public async void AppendMessageToLog(string message)
        {
            
            long id = IdProvider.GenerateId();
            var msg = new Message(id, message);

            var secondaryUrls = _configuration.GetSection("Secondaries:Urls").Get<List<string>>();

            var httpClient = _httpClientFactory.CreateClient();
            foreach(var secondaryUrl in secondaryUrls)
            {
                _logger.LogInformation("Master start replicating log to {secondaryUrl}", secondaryUrl);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var request = new HttpRequestMessage(HttpMethod.Post, $"{secondaryUrl}/api/log"))
                {
                    request.Content = new StringContent(JsonSerializer.Serialize(msg));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    try
                    {
                        var result = await httpClient.SendAsync(request);
                        result.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError("Error calling service {secondaryUrl}", secondaryUrl);
                        throw new ConnectionFailureException("Failed to connect to Secondary server.");
                    }
                }
            }
            
            _logger.LogInformation("Master append message to Log {message.Id}", msg.SequenceId);
            _repository.Add(msg);

        }

        public List<Message> GetAllMessages()
        {
            return _repository.GetAll();
        }
    }
}
