using ClietStockHub.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddStructuredLogging();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();
app.UseApiPipeline();

app.Run();
