using ClietStockHub.Api.Extensions;
using Microsoft.EntityFrameworkCore;


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
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ClietStockHub.Infrastructure.Persistence.AppDbContext>();
	db.Database.Migrate();
}
app.UseApiPipeline();

app.Run();
