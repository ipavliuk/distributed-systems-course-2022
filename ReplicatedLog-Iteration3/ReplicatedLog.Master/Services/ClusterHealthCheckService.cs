using ReplicatedLog.Master.HeartBeat;

namespace ReplicatedLog.Master.Services;

public class ClusterHealthCheckService : IHostedService
{
    private readonly IClusterHealthManager _clusterHealthManager;

    public ClusterHealthCheckService(IClusterHealthManager clusterHealthManager)
    {
        _clusterHealthManager = clusterHealthManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _clusterHealthManager.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _clusterHealthManager.Stop();

        return Task.CompletedTask;
    }
}
