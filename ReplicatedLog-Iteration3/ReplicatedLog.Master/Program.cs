using Common.Repository;
using ReplicatedLog.Master.HeartBeat;
using ReplicatedLog.Master.Services;
using ReplicatedLog.Common.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


builder.Services.AddSingleton<IRepository, InMemoryRepository>()
                .AddSingleton<IClusterHealthManager, ClusterHealthManager>()
                .AddSingleton<IReplicationBacklog, ReplicationBacklog>()
                .AddScoped<IReplicatedLogService, ReplicatedLogService>()
                .AddScoped<IMissedMessageReplicator, MissedMessageReplicator>();



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
