using ClietStockHub.Api.Common;
using ClietStockHub.Application;
using ClietStockHub.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ClietStockHub API",
        Version = "v1",
        Description = "API de Catálogo e Pedidos com envelope padrão de resposta."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ClietStockHub API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "ClietStockHub API Docs";
    });
}

app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Ok(ApiEnvelope.Success(new
{
    service = "ClietStockHub.Api",
    status = "running"
})));

app.Run();
