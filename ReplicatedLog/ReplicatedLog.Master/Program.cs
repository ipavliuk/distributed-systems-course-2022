using Common.Repository;
using ReplicatedLog.Master.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton<IRepository, InMemoryRepository>()
                .AddScoped<IReplicatedLogService, ReplicatedLogService>();
builder.Services.AddHttpClient();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
