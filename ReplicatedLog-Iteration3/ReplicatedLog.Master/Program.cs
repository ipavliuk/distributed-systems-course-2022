using Common.Repository;
using ReplicatedLog.Master.HeartBeat;
using ReplicatedLog.Master.Services.Options;
using ReplicatedLog.Master.Services.ReplicatedLogService;
using ReplicatedLog.Master.Services.MissedMessageReplicator;
using ReplicatedLog.Master.Services.HealthCheckService;
using ReplicatedLog.Common.ReplicationBacklog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


builder.Services.AddSingleton<IRepository, InMemoryRepository>()
                .AddSingleton<IClusterHealthManager, ClusterHealthManager>()
                .AddSingleton<IReplicationBacklog, ReplicationBacklog>()
                .AddScoped<IReplicatedLogService, ReplicatedLogService>()
                .AddSingleton<IMissedMessageReplicator, MissedMessageReplicator>();



builder.Services.Configure<RetryOptions>(
                builder.Configuration.GetSection("RetryPolicy")
    );

builder.Services.AddHostedService<ClusterHealthCheckService>();

builder.Services.AddHttpClient();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
