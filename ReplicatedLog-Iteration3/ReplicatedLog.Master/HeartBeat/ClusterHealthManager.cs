using ReplicatedLog.Master.Enums;
using ReplicatedLog.Master.Services;
using System.Collections.Concurrent;

namespace ReplicatedLog.Master.HeartBeat;

public class ClusterHealthManager: IClusterHealthManager
{
    private readonly ILogger<ClusterHealthManager> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, NodeStatus> _secondaryStatuses;
    private Timer _heartbeatTimer;
    private readonly IMissedMessageReplicator _missedMessageReplicator;

    public ClusterHealthManager(ILogger<ClusterHealthManager> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IMissedMessageReplicator missedMessageReplicator)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _secondaryStatuses = new ConcurrentDictionary<string, NodeStatus>();
        _missedMessageReplicator = missedMessageReplicator;
    }

    public Dictionary<string, NodeStatus> GetSecondariesStatus()
    {

        return _secondaryStatuses.ToDictionary( kvp => kvp.Key,
                                                kvp => kvp.Value);
    }

    private async void OnHeartbeatTimerElapsed(object state)
    {
        var secondaryUrls = _configuration.GetSection("Secondaries:Urls").Get<List<string>>();
        var httpClient = _httpClientFactory.CreateClient();

        foreach (var secondaryUrl in secondaryUrls)
        {
            try
            {
                var heartbeatRequest = new HttpRequestMessage(HttpMethod.Get, $"{secondaryUrl}/api/health");
                heartbeatRequest.Content = new StringContent(string.Empty);

                var prevStatus = _secondaryStatuses.ContainsKey(secondaryUrl) ? _secondaryStatuses[secondaryUrl] : NodeStatus.Healthy;
                var result = await httpClient.SendAsync(heartbeatRequest);

                if (result.IsSuccessStatusCode)
                {
                    if (prevStatus != NodeStatus.Healthy)
                    {
                        Task.Run(async () => await _missedMessageReplicator.ReplicateMissedMessagesAsync(secondaryUrl));
                    }
                    
                    _secondaryStatuses[secondaryUrl] = NodeStatus.Healthy;
                }
                else if (_secondaryStatuses.TryGetValue(secondaryUrl, out var status) && status == NodeStatus.Suspected)
                {
                    _secondaryStatuses[secondaryUrl] = NodeStatus.Unhealthy;
                    _logger.LogWarning("Failed to send heartbeat to secondary {secondaryUrl}, secondary is Unhealthy", secondaryUrl);
                }
                else
                {
                    _secondaryStatuses[secondaryUrl] = NodeStatus.Suspected;
                    _logger.LogWarning("Failed to send heartbeat to secondary {secondaryUrl}, secondary isn Suspected", secondaryUrl);
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error sending heartbeat to secondary {secondaryUrl}: {ex}", secondaryUrl, ex.Message);
                if (_secondaryStatuses.TryGetValue(secondaryUrl, out var status) && status == NodeStatus.Suspected)
                {
                    _secondaryStatuses[secondaryUrl] = NodeStatus.Unhealthy;
                }
                else
                {
                    _secondaryStatuses[secondaryUrl] = NodeStatus.Suspected;
                }
            }
        }
    }
    public bool IsNodeAvailable(string url)
    {
        return _secondaryStatuses.ContainsKey(url) ? _secondaryStatuses[url] == NodeStatus.Healthy : false;
    }

    public void Start()
    {
        _heartbeatTimer = new Timer(OnHeartbeatTimerElapsed, null, TimeSpan.Zero, TimeSpan.FromSeconds(60)); // TODO: add config time
    }

    public void Stop()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }
}
