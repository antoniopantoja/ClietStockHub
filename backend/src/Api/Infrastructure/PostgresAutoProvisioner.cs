using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ClietStockHub.Api.Infrastructure;

public static class PostgresAutoProvisioner
{
    public static async Task EnsurePostgresAvailableAsync(IConfiguration config, ILogger logger)
    {
        var connStr = config.GetConnectionString("DefaultConnection");
        if (await CanConnect(connStr))
        {
            logger.LogInformation("PostgreSQL já está disponível.");
            return;
        }

        logger.LogWarning("PostgreSQL não encontrado. Tentando subir container Docker...");
        await StartPostgresContainer(logger);

        // Aguarda o banco subir
        for (int i = 0; i < 10; i++)
        {
            if (await CanConnect(connStr))
            {
                logger.LogInformation("PostgreSQL disponível após provisionamento.");
                return;
            }
            await Task.Delay(2000);
        }
        throw new Exception("Não foi possível provisionar o PostgreSQL automaticamente.");
    }

    private static async Task<bool> CanConnect(string connStr)
    {
        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task StartPostgresContainer(ILogger logger)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "run --name csh-postgres -e POSTGRES_PASSWORD=csh_pass -e POSTGRES_USER=csh_user -e POSTGRES_DB=csh_db -p 5432:5432 -d postgres:16",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        logger.LogInformation($"docker run output: {output}");
        if (!string.IsNullOrWhiteSpace(error))
            logger.LogWarning($"docker run error: {error}");
    }
}
