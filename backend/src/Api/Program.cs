using ClietStockHub.Api.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Auto-provisionamento do PostgreSQL em dev
if (builder.Environment.IsDevelopment())
{
	var logger = LoggerFactory.Create(cfg => cfg.AddConsole()).CreateLogger("AutoProvision");
	await ClietStockHub.Api.Infrastructure.PostgresAutoProvisioner.EnsurePostgresAvailableAsync(builder.Configuration, logger);
}

builder.Host.AddStructuredLogging();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();
app.UseApiPipeline();

app.Run();
